import {
  Stack,
  Text,
  Group,
  Button,
  Image,
  ActionIcon,
  Box,
  Alert,
  Paper,
} from '@mantine/core';
import { Dropzone, IMAGE_MIME_TYPE } from '@mantine/dropzone';
import {
  FiUpload,
  FiTrash2,
  FiImage,
  FiX,
  FiAlertCircle,
  FiMove,
} from 'react-icons/fi';
import { useProductPhotos } from '../../../lib/hooks/useProductPhotos';
import { ProductPhotoDto } from '../../../lib/types';
import { notifications } from '@mantine/notifications';
import { useState } from 'react';
import {
  DragDropContext,
  Droppable,
  Draggable,
  DropResult,
} from '@hello-pangea/dnd';

interface ProductPhotosSectionProps {
  productId?: number;
  photos: ProductPhotoDto[];
  isEditing: boolean;
}

const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;

export function ProductPhotosSection({
  productId,
  photos,
  isEditing,
}: ProductPhotosSectionProps) {
  const { addProductPhoto, deleteProductPhoto, updatePhotoDisplayOrder } =
    useProductPhotos();
  const [uploadingFiles, setUploadingFiles] = useState<string[]>([]);

  const handleFileDrop = async (files: File[]) => {
    if (!productId) {
      notifications.show({
        title: 'Cannot upload photos',
        message: 'Please save the product first before adding photos',
        color: 'orange',
      });
      return;
    }

    for (const file of files) {
      const fileId = `${file.name}-${Date.now()}`;
      setUploadingFiles((prev) => [...prev, fileId]);

      try {
        await addProductPhoto.mutateAsync({ productId, file });
        notifications.show({
          title: 'Photo uploaded',
          message: `${file.name} has been uploaded successfully`,
          color: 'green',
        });
      } catch (error) {
        console.error('Error uploading photo:', error);
        notifications.show({
          title: 'Upload failed',
          message: `Failed to upload ${file.name}. Please try again.`,
          color: 'red',
        });
      } finally {
        setUploadingFiles((prev) => prev.filter((id) => id !== fileId));
      }
    }
  };

  const handleDeletePhoto = async (photoKey: string) => {
    if (!productId) return;

    try {
      await deleteProductPhoto.mutateAsync({ productId, photoKey });
      notifications.show({
        title: 'Photo deleted',
        message: 'Photo has been deleted successfully',
        color: 'green',
      });
    } catch (error) {
      console.error('Error deleting photo:', error);
      notifications.show({
        title: 'Delete failed',
        message: 'Failed to delete photo. Please try again.',
        color: 'red',
      });
    }
  };

  const handleDragEnd = async (result: DropResult) => {
    if (!result.destination || !productId) return;

    const sourceIndex = result.source.index;
    const destinationIndex = result.destination.index;

    if (sourceIndex === destinationIndex) return;

    // Create a new array with reordered photos
    const reorderedPhotos = Array.from(photos);
    const [removed] = reorderedPhotos.splice(sourceIndex, 1);
    reorderedPhotos.splice(destinationIndex, 0, removed);

    // Create the update payload with new display orders
    const photoOrders = reorderedPhotos.map((photo, index) => ({
      key: photo.key,
      displayOrder: index + 1,
    }));

    try {
      await updatePhotoDisplayOrder.mutateAsync({ productId, photoOrders });
      notifications.show({
        title: 'Photos reordered',
        message: 'Photo order has been updated successfully',
        color: 'green',
      });
    } catch (error) {
      console.error('Error reordering photos:', error);
      notifications.show({
        title: 'Reorder failed',
        message: 'Failed to reorder photos. Please try again.',
        color: 'red',
      });
    }
  };

  if (!isEditing) {
    return (
      <Stack gap="md">
        <Text c="dimmed">Save the product first to add and manage photos</Text>
        <Button variant="outline" disabled>
          Upload Photos
        </Button>
      </Stack>
    );
  }

  return (
    <Stack gap="md">
      <Text c="dimmed" size="sm">
        Upload high-quality images to showcase your product. The first image
        will be used as the main product photo.
      </Text>

      {/* Photo Upload Dropzone */}
      <Dropzone
        onDrop={handleFileDrop}
        onReject={(files) => {
          files.forEach((file) => {
            notifications.show({
              title: 'File rejected',
              message: `${file.file.name}: ${file.errors[0]?.message}`,
              color: 'red',
            });
          });
        }}
        maxSize={10 * 1024 ** 2} // 10MB
        accept={IMAGE_MIME_TYPE}
        loading={addProductPhoto.isPending || uploadingFiles.length > 0}
      >
        <Group
          justify="center"
          gap="xl"
          mih={120}
          style={{ pointerEvents: 'none' }}
        >
          <Dropzone.Accept>
            <FiUpload size={52} color="var(--mantine-color-blue-6)" />
          </Dropzone.Accept>
          <Dropzone.Reject>
            <FiX size={52} color="var(--mantine-color-red-6)" />
          </Dropzone.Reject>
          <Dropzone.Idle>
            <FiImage size={52} color="var(--mantine-color-dimmed)" />
          </Dropzone.Idle>

          <div>
            <Text size="xl" inline>
              Drag images here or click to select files
            </Text>
            <Text size="sm" c="dimmed" inline mt={7}>
              Attach up to 10 images, each file should not exceed 10MB
            </Text>
          </div>
        </Group>
      </Dropzone>

      {/* Uploaded Photos Grid */}
      {photos.length > 0 && (
        <Stack gap="xs">
          <Group justify="space-between">
            <Text size="sm" fw={500}>
              Uploaded Photos ({photos.length})
            </Text>
            <Text size="xs" c="dimmed">
              Drag to reorder • First image is the main photo
            </Text>
          </Group>

          <DragDropContext onDragEnd={handleDragEnd}>
            <Droppable droppableId="photos" direction="horizontal">
              {(provided) => (
                <Box
                  {...provided.droppableProps}
                  ref={provided.innerRef}
                  style={{ overflowX: 'auto' }}
                >
                  <Group gap="md" wrap="nowrap">
                    {photos
                      .sort((a, b) => a.displayOrder - b.displayOrder)
                      .map((photo, index) => (
                        <Draggable
                          key={photo.key}
                          draggableId={photo.key}
                          index={index}
                        >
                          {(provided, snapshot) => (
                            <Paper
                              ref={provided.innerRef}
                              {...provided.draggableProps}
                              style={{
                                ...provided.draggableProps.style,
                                opacity: snapshot.isDragging ? 0.8 : 1,
                                minWidth: 120,
                                transform: snapshot.isDragging
                                  ? `${provided.draggableProps.style?.transform} rotate(5deg)`
                                  : provided.draggableProps.style?.transform,
                              }}
                              p="xs"
                              withBorder
                              shadow={snapshot.isDragging ? 'lg' : 'sm'}
                            >
                              <Stack gap="xs" align="center">
                                <Box pos="relative">
                                  <Image
                                    src={`${baseImageUrl}${photo.key}`}
                                    alt="Product photo"
                                    w={100}
                                    h={100}
                                    fit="cover"
                                    radius="sm"
                                  />
                                  {index === 0 && (
                                    <Box
                                      pos="absolute"
                                      top={4}
                                      left={4}
                                      bg="blue"
                                      c="white"
                                      px={4}
                                      py={2}
                                      style={{
                                        borderRadius: '4px',
                                        fontSize: '10px',
                                        fontWeight: 600,
                                      }}
                                    >
                                      MAIN
                                    </Box>
                                  )}
                                  <ActionIcon
                                    pos="absolute"
                                    top={4}
                                    right={4}
                                    size="sm"
                                    color="red"
                                    onClick={() => handleDeletePhoto(photo.key)}
                                    loading={deleteProductPhoto.isPending}
                                  >
                                    <FiTrash2 size={12} />
                                  </ActionIcon>
                                </Box>
                                <Group gap={4} justify="center">
                                  <Box
                                    {...provided.dragHandleProps}
                                    style={{
                                      cursor: 'grab',
                                      padding: '4px',
                                      borderRadius: '4px',
                                      display: 'flex',
                                      alignItems: 'center',
                                      justifyContent: 'center',
                                    }}
                                  >
                                    <FiMove
                                      size={14}
                                      color="var(--mantine-color-gray-6)"
                                    />
                                  </Box>
                                  <Text size="xs" c="dimmed">
                                    #{photo.displayOrder}
                                  </Text>
                                </Group>
                              </Stack>
                            </Paper>
                          )}
                        </Draggable>
                      ))}
                    {provided.placeholder}
                  </Group>
                </Box>
              )}
            </Droppable>
          </DragDropContext>
        </Stack>
      )}

      {/* Info Alert */}
      <Alert
        icon={<FiAlertCircle size={16} />}
        title="Photo Guidelines"
        color="blue"
        variant="light"
      >
        <Text size="sm">
          • The first photo will be the main image shown in listings
          <br />• Supported formats: JPG, PNG.
          <br />• Show your product from multiple angles
        </Text>
      </Alert>
    </Stack>
  );
}
