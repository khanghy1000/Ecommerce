import { useState } from 'react';
import {
  Container,
  Title,
  Paper,
  Table,
  Button,
  Text,
  Group,
  ActionIcon,
  Badge,
  Box,
  Tabs,
  Modal,
  Flex,
  Loader,
  Alert,
  ScrollArea,
  Stack,
  Accordion,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useCategories } from '../../../lib/hooks/useCategories';
import {
  CreateCategoryRequestDto,
  EditCategoryRequestDto,
  CreateSubcategoryRequestDto,
  EditSubcategoryRequestDto,
  CategoryResponseDto,
  SubcategoryResponseDto,
} from '../../../lib/types';
import { FiPlus, FiEdit, FiTrash2, FiAlertCircle } from 'react-icons/fi';
import { notifications } from '@mantine/notifications';
import { CategoryForm } from './CategoryForm';
import { SubcategoryForm } from './SubcategoryForm';

function CategoriesManagementPage() {
  // State for modals
  const [opened, { open, close }] = useDisclosure(false);
  const [modalType, setModalType] = useState<
    'addCategory' | 'editCategory' | 'addSubcategory' | 'editSubcategory'
  >('addCategory');
  const [selectedCategory, setSelectedCategory] = useState<number | null>(null);
  const [selectedSubcategory, setSelectedSubcategory] = useState<number | null>(
    null
  );
  const [activeTab, setActiveTab] = useState<string | null>('categories');
  const [
    deleteConfirmOpened,
    { open: openDeleteConfirm, close: closeDeleteConfirm },
  ] = useDisclosure(false);
  const [itemToDelete, setItemToDelete] = useState<{
    id: number;
    type: 'category' | 'subcategory';
  } | null>(null);

  // Fetch categories and related data
  const {
    categories,
    loadingCategories,
    createCategory,
    editCategory,
    deleteCategory,
    createSubcategory,
    editSubcategory,
    deleteSubcategory,
    subcategory,
  } = useCategories(
    selectedCategory || undefined,
    selectedSubcategory || undefined
  );

  // Initial form values
  const initialCategoryValues: CreateCategoryRequestDto = {
    name: '',
  };

  const initialSubcategoryValues: CreateSubcategoryRequestDto = {
    name: '',
    categoryId: selectedCategory || 0,
  };

  // Handler for opening modal with different forms
  const handleOpenModal = (
    type: typeof modalType,
    categoryId?: number,
    subcategoryId?: number
  ) => {
    setModalType(type);
    if (categoryId) setSelectedCategory(categoryId);
    if (subcategoryId) setSelectedSubcategory(subcategoryId);
    open();
  };

  // Handler for confirming delete
  const handleConfirmDelete = (
    id: number,
    type: 'category' | 'subcategory'
  ) => {
    setItemToDelete({ id, type });
    openDeleteConfirm();
  };

  // Handler for performing delete
  const handleDelete = async () => {
    if (!itemToDelete) return;

    try {
      if (itemToDelete.type === 'category') {
        await deleteCategory.mutateAsync(itemToDelete.id);
        notifications.show({
          title: 'Success',
          message: 'Category deleted successfully',
          color: 'green',
        });
      } else {
        await deleteSubcategory.mutateAsync(itemToDelete.id);
        notifications.show({
          title: 'Success',
          message: 'Subcategory deleted successfully',
          color: 'green',
        });
      }
    } catch (error) {
      console.error('Delete error:', error);
      notifications.show({
        title: 'Error',
        message: 'Failed to delete item',
        color: 'red',
      });
    } finally {
      closeDeleteConfirm();
      setItemToDelete(null);
    }
  };

  // Handlers for form submissions
  const handleCategorySubmit = async (values: CreateCategoryRequestDto) => {
    try {
      if (modalType === 'addCategory') {
        await createCategory.mutateAsync(values);
        notifications.show({
          title: 'Success',
          message: 'Category created successfully',
          color: 'green',
        });
      } else {
        if (selectedCategory) {
          await editCategory.mutateAsync({
            id: selectedCategory,
            categoryData: values as EditCategoryRequestDto,
          });
          notifications.show({
            title: 'Success',
            message: 'Category updated successfully',
            color: 'green',
          });
        }
      }
      close();
    } catch (error) {
      console.error('Submit error:', error);
      notifications.show({
        title: 'Error',
        message: 'Failed to save category',
        color: 'red',
      });
    }
  };

  const handleSubcategorySubmit = async (
    values: CreateSubcategoryRequestDto
  ) => {
    try {
      if (modalType === 'addSubcategory') {
        await createSubcategory.mutateAsync(values);
        notifications.show({
          title: 'Success',
          message: 'Subcategory created successfully',
          color: 'green',
        });
      } else {
        if (selectedSubcategory) {
          await editSubcategory.mutateAsync({
            id: selectedSubcategory,
            subcategoryData: values as EditSubcategoryRequestDto,
          });
          notifications.show({
            title: 'Success',
            message: 'Subcategory updated successfully',
            color: 'green',
          });
        }
      }
      close();
    } catch (error) {
      console.error('Submit error:', error);
      notifications.show({
        title: 'Error',
        message: 'Failed to save subcategory',
        color: 'red',
      });
    }
  };

  // Determine loading states
  const isSubmitting =
    createCategory.isPending ||
    editCategory.isPending ||
    deleteCategory.isPending ||
    createSubcategory.isPending ||
    editSubcategory.isPending ||
    deleteSubcategory.isPending;

  // Form getters for edit mode
  const getCategoryFormValues = () => {
    if (modalType === 'editCategory' && selectedCategory) {
      const cat = categories?.find((c) => c.id === selectedCategory);
      if (cat) {
        return { name: cat.name };
      }
    }
    return initialCategoryValues;
  };

  const getSubcategoryFormValues = () => {
    if (modalType === 'editSubcategory' && selectedSubcategory && subcategory) {
      return {
        name: subcategory.name,
        categoryId: subcategory.categoryId,
      };
    }
    return initialSubcategoryValues;
  };

  // Render categories table
  const renderCategoriesTable = () => {
    if (loadingCategories) {
      return (
        <Flex justify="center" align="center" h={200}>
          <Loader />
        </Flex>
      );
    }

    if (!categories || categories.length === 0) {
      return (
        <Alert
          icon={<FiAlertCircle size={16} />}
          title="No categories"
          color="blue"
        >
          No categories found. Click the "Add Category" button to create one.
        </Alert>
      );
    }

    return (
      <ScrollArea h={500}>
        <Table striped highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>ID</Table.Th>
              <Table.Th>Name</Table.Th>
              <Table.Th>Subcategories</Table.Th>
              <Table.Th style={{ width: 120 }}>Actions</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {categories.map((category) => (
              <Table.Tr key={category.id}>
                <Table.Td>{category.id}</Table.Td>
                <Table.Td>{category.name}</Table.Td>
                <Table.Td>
                  <Group gap={5} wrap="wrap">
                    {category.subcategories.map((subcat) => (
                      <Badge variant='default' key={subcat.id}>{subcat.name}</Badge>
                    ))}
                    {category.subcategories.length === 0 && (
                      <Text size="sm" c="dimmed">
                        None
                      </Text>
                    )}
                  </Group>
                </Table.Td>
                <Table.Td>
                  <Group gap={5}>
                    <ActionIcon
                      color="blue"
                      variant="subtle"
                      onClick={() =>
                        handleOpenModal('editCategory', category.id)
                      }
                      aria-label="Edit category"
                    >
                      <FiEdit size={16} />
                    </ActionIcon>
                    <ActionIcon
                      color="green"
                      variant="subtle"
                      onClick={() => {
                        setSelectedCategory(category.id);
                        handleOpenModal('addSubcategory', category.id);
                      }}
                      aria-label="Add subcategory"
                    >
                      <FiPlus size={16} />
                    </ActionIcon>
                    <ActionIcon
                      color="red"
                      variant="subtle"
                      onClick={() =>
                        handleConfirmDelete(category.id, 'category')
                      }
                      aria-label="Delete category"
                    >
                      <FiTrash2 size={16} />
                    </ActionIcon>
                  </Group>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      </ScrollArea>
    );
  };

  // Render subcategories view (grouped by category)
  const renderSubcategoriesView = () => {
    if (loadingCategories) {
      return (
        <Flex justify="center" align="center" h={200}>
          <Loader />
        </Flex>
      );
    }

    if (!categories || categories.length === 0) {
      return (
        <Alert
          icon={<FiAlertCircle size={16} />}
          title="No categories"
          color="blue"
        >
          No categories found. Create categories first before adding
          subcategories.
        </Alert>
      );
    }

    if (categories.every((cat) => cat.subcategories.length === 0)) {
      return (
        <Alert
          icon={<FiAlertCircle size={16} />}
          title="No subcategories"
          color="blue"
        >
          No subcategories found. Add subcategories to categories first.
        </Alert>
      );
    }

    return (
      <Accordion>
        {categories.map(
          (category) =>
            category.subcategories.length > 0 && (
              <Accordion.Item key={category.id} value={category.id.toString()}>
                <Accordion.Control>
                  {category.name} ({category.subcategories.length}{' '}
                  subcategories)
                </Accordion.Control>
                <Accordion.Panel>
                  <Table>
                    <Table.Thead>
                      <Table.Tr>
                        <Table.Th>ID</Table.Th>
                        <Table.Th>Name</Table.Th>
                        <Table.Th style={{ width: 120 }}>Actions</Table.Th>
                      </Table.Tr>
                    </Table.Thead>
                    <Table.Tbody>
                      {category.subcategories.map((subcat) => (
                        <Table.Tr key={subcat.id}>
                          <Table.Td>{subcat.id}</Table.Td>
                          <Table.Td>{subcat.name}</Table.Td>
                          <Table.Td>
                            <Group gap={5}>
                              <ActionIcon
                                color="blue"
                                variant="subtle"
                                onClick={() =>
                                  handleOpenModal(
                                    'editSubcategory',
                                    category.id,
                                    subcat.id
                                  )
                                }
                                aria-label="Edit subcategory"
                              >
                                <FiEdit size={16} />
                              </ActionIcon>
                              <ActionIcon
                                color="red"
                                variant="subtle"
                                onClick={() =>
                                  handleConfirmDelete(subcat.id, 'subcategory')
                                }
                                aria-label="Delete subcategory"
                              >
                                <FiTrash2 size={16} />
                              </ActionIcon>
                            </Group>
                          </Table.Td>
                        </Table.Tr>
                      ))}
                    </Table.Tbody>
                  </Table>
                </Accordion.Panel>
              </Accordion.Item>
            )
        )}
      </Accordion>
    );
  };

  return (
    <Container size="lg">
      <Title order={2} mb="lg">
        Categories Management
      </Title>

      <Paper p="md" withBorder mb="lg">
        <Group justify="space-between" mb="md">
          <Tabs value={activeTab} onChange={setActiveTab}>
            <Tabs.List>
              <Tabs.Tab value="categories">Categories</Tabs.Tab>
              <Tabs.Tab value="subcategories">Subcategories</Tabs.Tab>
            </Tabs.List>
          </Tabs>

          <Group>
            {activeTab === 'categories' && (
              <Button
                leftSection={<FiPlus size={16} />}
                onClick={() => handleOpenModal('addCategory')}
              >
                Add Category
              </Button>
            )}
          </Group>
        </Group>

        {/* Tab Content */}
        <Box>
          {activeTab === 'categories'
            ? renderCategoriesTable()
            : renderSubcategoriesView()}
        </Box>
      </Paper>

      {/* Modal for adding/editing categories and subcategories */}
      <Modal
        opened={opened}
        onClose={close}
        title={
          modalType === 'addCategory'
            ? 'Add Category'
            : modalType === 'editCategory'
              ? 'Edit Category'
              : modalType === 'addSubcategory'
                ? 'Add Subcategory'
                : 'Edit Subcategory'
        }
      >
        {(modalType === 'addCategory' || modalType === 'editCategory') && (
          <CategoryForm
            initialValues={getCategoryFormValues()}
            onSubmit={handleCategorySubmit}
            isSubmitting={isSubmitting}
            submitLabel={
              modalType === 'addCategory' ? 'Add Category' : 'Update Category'
            }
          />
        )}

        {(modalType === 'addSubcategory' ||
          modalType === 'editSubcategory') && (
          <SubcategoryForm
            initialValues={getSubcategoryFormValues()}
            onSubmit={handleSubcategorySubmit}
            isSubmitting={isSubmitting}
            submitLabel={
              modalType === 'addSubcategory'
                ? 'Add Subcategory'
                : 'Update Subcategory'
            }
            categories={categories}
          />
        )}
      </Modal>

      {/* Confirmation Modal for Delete */}
      <Modal
        opened={deleteConfirmOpened}
        onClose={closeDeleteConfirm}
        title="Confirm Deletion"
      >
        <Stack>
          <Text>
            Are you sure you want to delete this {itemToDelete?.type}? This
            action cannot be undone.
            {itemToDelete?.type === 'category' &&
              ' All subcategories will also be deleted.'}
          </Text>

          <Group justify="flex-end">
            <Button variant="outline" onClick={closeDeleteConfirm}>
              Cancel
            </Button>
            <Button color="red" onClick={handleDelete} loading={isSubmitting}>
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Container>
  );
}

export default CategoriesManagementPage;
