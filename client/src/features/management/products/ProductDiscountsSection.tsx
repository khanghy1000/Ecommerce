import {
  Stack,
  Button,
  Table,
  Group,
  Text,
  ActionIcon,
  Modal,
  NumberInput,
  Badge,
  Alert,
  Loader,
  Center,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { FiEdit2, FiTrash2, FiPlus, FiClock, FiInfo } from 'react-icons/fi';
import { useState } from 'react';
import { notifications } from '@mantine/notifications';
import { DateTimePicker } from '@mantine/dates';
import { useForm, zodResolver } from '@mantine/form';
import { z } from 'zod';
import { useProductDiscounts } from '../../../lib/hooks/useProductDiscounts';
import { format } from 'date-fns';
import {
  AddProductDiscountRequestDto,
  EditProductDiscountRequestDto,
  ProductDiscountResponseDto,
} from '../../../lib/types';

interface ProductDiscountsSectionProps {
  productId?: number;
  regularPrice: number;
  isEditing: boolean;
}

const discountSchema = z
  .object({
    discountPrice: z
      .number()
      .positive('Discount price must be positive')
      .min(1000, 'Discount price must be at least 1,000 ₫'),
    startTime: z.union([
      z.date(),
      z.string().transform((val) => new Date(val)),
    ]),
    endTime: z.union([z.date(), z.string().transform((val) => new Date(val))]),
  })
  .refine(
    (data) => {
      const startTime =
        data.startTime instanceof Date
          ? data.startTime
          : new Date(data.startTime);
      const endTime =
        data.endTime instanceof Date ? data.endTime : new Date(data.endTime);
      return endTime > startTime;
    },
    {
      message: 'End time must be after start time',
      path: ['endTime'],
    }
  );

type DiscountFormValues = z.infer<typeof discountSchema>;

function ProductDiscountsSection({
  productId,
  regularPrice,
  isEditing,
}: ProductDiscountsSectionProps) {
  const [modalOpened, { open: openModal, close: closeModal }] =
    useDisclosure(false);
  const [
    deleteModalOpened,
    { open: openDeleteModal, close: closeDeleteModal },
  ] = useDisclosure(false);
  const [editingDiscount, setEditingDiscount] =
    useState<ProductDiscountResponseDto | null>(null);
  const [deletingDiscount, setDeletingDiscount] =
    useState<ProductDiscountResponseDto | null>(null);

  const {
    productDiscounts,
    loadingProductDiscounts,
    addProductDiscount,
    editProductDiscount,
    deleteProductDiscount,
  } = useProductDiscounts(productId);

  const form = useForm<DiscountFormValues>({
    validate: zodResolver(
      discountSchema.refine((data) => data.discountPrice < regularPrice, {
        message: 'Discount price must be less than regular price',
        path: ['discountPrice'],
      })
    ),
    initialValues: {
      discountPrice: Math.floor(regularPrice * 0.8),
      startTime: new Date(),
      endTime: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000), // 7 days from now
    },
  });

  const handleSubmit = (values: DiscountFormValues) => {
    if (!productId) return;

    const requestData = {
      discountPrice: values.discountPrice,
      startTime:
        values.startTime instanceof Date
          ? values.startTime
          : new Date(values.startTime),
      endTime:
        values.endTime instanceof Date
          ? values.endTime
          : new Date(values.endTime),
    };

    if (editingDiscount) {
      editProductDiscount.mutate(
        {
          productId,
          discountId: editingDiscount.id,
          discountData: requestData as EditProductDiscountRequestDto,
        },
        {
          onSuccess: () => {
            notifications.show({
              title: 'Discount updated',
              message: 'Product discount has been updated successfully',
              color: 'green',
            });
            handleCloseModal();
          },
          onError: (error: unknown) => {
            notifications.show({
              title: 'Error',
              message:
                error instanceof Error
                  ? error.message
                  : 'Failed to update discount. Please try again.',
              color: 'red',
            });
          },
        }
      );
    } else {
      addProductDiscount.mutate(
        {
          productId,
          discountData: requestData as AddProductDiscountRequestDto,
        },
        {
          onSuccess: () => {
            notifications.show({
              title: 'Discount added',
              message: 'Product discount has been added successfully',
              color: 'green',
            });
            handleCloseModal();
          },
          onError: (error: unknown) => {
            console.error('Error adding discount:', error);
            notifications.show({
              title: 'Error',
              message:
                error instanceof Error
                  ? error.message
                  : 'Failed to add discount. Please try again.',
              color: 'red',
            });
          },
        }
      );
    }
  };

  const handleEdit = (discount: ProductDiscountResponseDto) => {
    setEditingDiscount(discount);
    form.setValues({
      discountPrice: discount.discountPrice,
      startTime: new Date(discount.startTime),
      endTime: new Date(discount.endTime),
    });
    openModal();
  };

  const handleDelete = (discount: ProductDiscountResponseDto) => {
    setDeletingDiscount(discount);
    openDeleteModal();
  };

  const confirmDelete = () => {
    if (!deletingDiscount || !productId) return;

    deleteProductDiscount.mutate(
      {
        productId,
        discountId: deletingDiscount.id,
      },
      {
        onSuccess: () => {
          notifications.show({
            title: 'Discount deleted',
            message: 'Product discount has been deleted successfully',
            color: 'green',
          });
          closeDeleteModal();
          setDeletingDiscount(null);
        },
        onError: (error: unknown) => {
          console.error('Error deleting discount:', error);
          notifications.show({
            title: 'Error',
            message:
              error instanceof Error
                ? error.message
                : 'Failed to delete discount. Please try again.',
            color: 'red',
          });
        },
      }
    );
  };

  const handleCloseModal = () => {
    closeModal();
    setEditingDiscount(null);
    form.reset();
  };

  const getDiscountStatus = (discount: ProductDiscountResponseDto) => {
    const now = new Date();
    const startTime = new Date(discount.startTime);
    const endTime = new Date(discount.endTime);

    if (now < startTime) {
      return { status: 'upcoming', color: 'blue', label: 'Upcoming' };
    } else if (now >= startTime && now <= endTime) {
      return { status: 'active', color: 'green', label: 'Active' };
    } else {
      return { status: 'expired', color: 'gray', label: 'Expired' };
    }
  };

  const calculateDiscountPercentage = (discountPrice: number) => {
    return Math.round(((regularPrice - discountPrice) / regularPrice) * 100);
  };

  if (!isEditing) {
    return (
      <Alert icon={<FiInfo size={16} />} color="blue">
        Discount management is available after creating the product.
      </Alert>
    );
  }

  if (loadingProductDiscounts) {
    return (
      <Center py="xl">
        <Loader size="md" />
      </Center>
    );
  }

  return (
    <Stack gap="md">
      <Group justify="space-between" align="center">
        <Text size="sm" c="dimmed">
          {productDiscounts?.length || 0} discount(s) configured
        </Text>
        <Button
          leftSection={<FiPlus size={16} />}
          variant="outline"
          onClick={() => {
            setEditingDiscount(null);
            form.reset();
            openModal();
          }}
        >
          Add Discount
        </Button>
      </Group>

      {productDiscounts && productDiscounts.length > 0 ? (
        <Table striped highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Discount Price</Table.Th>
              <Table.Th>Percentage</Table.Th>
              <Table.Th>Period</Table.Th>
              <Table.Th>Status</Table.Th>
              <Table.Th>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {productDiscounts.map((discount) => {
              const discountStatus = getDiscountStatus(discount);
              const discountPercentage = calculateDiscountPercentage(
                discount.discountPrice
              );

              return (
                <Table.Tr key={discount.id}>
                  <Table.Td>
                    <Text fw={500}>
                      {discount.discountPrice.toLocaleString('vi-VN')} ₫
                    </Text>
                    <Text size="sm" c="dimmed">
                      Regular: {regularPrice.toLocaleString('vi-VN')} ₫
                    </Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge color="orange" variant="light">
                      -{discountPercentage}%
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    <Group gap="xs" align="center">
                      <FiClock size={14} />
                      <Stack gap={2}>
                        <Text size="sm">
                          {format(
                            new Date(discount.startTime),
                            'MMM dd, yyyy HH:mm'
                          )}
                        </Text>
                        <Text size="sm" c="dimmed">
                          to{' '}
                          {format(
                            new Date(discount.endTime),
                            'MMM dd, yyyy HH:mm'
                          )}
                        </Text>
                      </Stack>
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={discountStatus.color} variant="light">
                      {discountStatus.label}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    <Group gap="xs">
                      <ActionIcon
                        variant="subtle"
                        color="blue"
                        size="sm"
                        onClick={() => handleEdit(discount)}
                      >
                        <FiEdit2 size={14} />
                      </ActionIcon>
                      <ActionIcon
                        variant="subtle"
                        color="red"
                        size="sm"
                        onClick={() => handleDelete(discount)}
                      >
                        <FiTrash2 size={14} />
                      </ActionIcon>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              );
            })}
          </Table.Tbody>
        </Table>
      ) : (
        <Alert icon={<FiInfo size={16} />} color="gray">
          No discounts configured for this product. Click "Add Discount" to
          create your first discount.
        </Alert>
      )}

      {/* Add/Edit Discount Modal */}
      <Modal
        opened={modalOpened}
        onClose={handleCloseModal}
        title={editingDiscount ? 'Edit Discount' : 'Add New Discount'}
        size="md"
      >
        <form
          onSubmit={form.onSubmit(handleSubmit)}
          onReset={(e) => e.preventDefault()}
        >
          <Stack gap="md">
            <NumberInput
              label="Discount Price"
              placeholder="800,000"
              description={`Must be less than regular price (${regularPrice.toLocaleString('vi-VN')} ₫)`}
              required
              min={1000}
              max={regularPrice - 1000}
              step={1000}
              suffix=" ₫"
              {...form.getInputProps('discountPrice')}
            />

            <Group grow>
              <DateTimePicker
                label="Start Time"
                placeholder="Select start date and time"
                required
                minDate={new Date()}
                {...form.getInputProps('startTime')}
              />
              <DateTimePicker
                label="End Time"
                placeholder="Select end date and time"
                required
                minDate={form.values.startTime}
                {...form.getInputProps('endTime')}
              />
            </Group>

            {form.values.discountPrice > 0 &&
              form.values.discountPrice < regularPrice && (
                <Alert icon={<FiInfo size={16} />} color="green">
                  This discount will save customers{' '}
                  <Text component="span" fw={700}>
                    {calculateDiscountPercentage(form.values.discountPrice)}%
                  </Text>{' '}
                  (
                  {(regularPrice - form.values.discountPrice).toLocaleString(
                    'vi-VN'
                  )}{' '}
                  ₫)
                </Alert>
              )}

            <Group justify="flex-end" mt="md">
              <Button variant="outline" onClick={handleCloseModal}>
                Cancel
              </Button>
              <Button
                type="button"
                onClick={() => {
                  const validation = form.validate();
                  if (!validation.hasErrors) {
                    handleSubmit(form.getValues());
                  }
                }}
                loading={
                  addProductDiscount.isPending || editProductDiscount.isPending
                }
              >
                {editingDiscount ? 'Update Discount' : 'Add Discount'}
              </Button>
            </Group>
          </Stack>
        </form>
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal
        opened={deleteModalOpened}
        onClose={() => {
          closeDeleteModal();
          setDeletingDiscount(null);
        }}
        title="Delete Discount"
        size="sm"
      >
        <Stack gap="md">
          <Text>
            Are you sure you want to delete this discount? This action cannot be
            undone.
          </Text>
          {deletingDiscount && (
            <Alert color="orange">
              <Text size="sm">
                <strong>Discount:</strong>{' '}
                {deletingDiscount.discountPrice.toLocaleString('vi-VN')} ₫
                <br />
                <strong>Period:</strong>{' '}
                {format(new Date(deletingDiscount.startTime), 'MMM dd, yyyy')} -{' '}
                {format(new Date(deletingDiscount.endTime), 'MMM dd, yyyy')}
              </Text>
            </Alert>
          )}
          <Group justify="flex-end">
            <Button
              variant="outline"
              onClick={() => {
                closeDeleteModal();
                setDeletingDiscount(null);
              }}
            >
              Cancel
            </Button>
            <Button
              color="red"
              loading={deleteProductDiscount.isPending}
              onClick={confirmDelete}
            >
              Delete Discount
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  );
}

export { ProductDiscountsSection };
