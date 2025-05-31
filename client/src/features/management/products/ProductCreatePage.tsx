import {
  Container,
  Title,
  Paper,
  Group,
  Button,
  Loader,
  Text,
} from '@mantine/core';
import { Link, useNavigate } from 'react-router';
import { FiArrowLeft } from 'react-icons/fi';
import { useProducts } from '../../../lib/hooks/useProducts';
import { useCategories } from '../../../lib/hooks/useCategories';
import { CreateProductRequestDto } from '../../../lib/types';
import { notifications } from '@mantine/notifications';
import ProductForm from './ProductForm';

function ProductCreatePage() {
  const navigate = useNavigate();
  const { createProduct } = useProducts();
  const { categories, loadingCategories } = useCategories();

  const handleSubmit = (data: CreateProductRequestDto) => {
    createProduct.mutate(data, {
      onSuccess: (createdProduct) => {
        notifications.show({
          title: 'Product created',
          message: `Product "${createdProduct.name}" has been created successfully`,
          color: 'green',
        });
        navigate(`/management/products/edit/${createdProduct.id}`);
      },
      onError: (error: unknown) => {
        console.error('Error creating product:', error);
        notifications.show({
          title: 'Error',
          message:
            error instanceof Error
              ? error.message
              : 'Failed to create product. Please try again.',
          color: 'red',
        });
      },
    });
  };

  if (loadingCategories) {
    return (
      <Container size="lg" py="xl">
        <Group justify="center">
          <Loader size="lg" />
          <Text>Loading categories...</Text>
        </Group>
      </Container>
    );
  }

  return (
    <Container size="lg" py="xl">
      <Button
        component={Link}
        to="/management/products"
        variant="subtle"
        leftSection={<FiArrowLeft size={16} />}
      >
        Back to Products
      </Button>
      <Title order={2} mt="lg" mb="lg">
        Create New Product
      </Title>

      <Paper p="xl" withBorder>
        <ProductForm
          onSubmit={handleSubmit}
          categories={categories || []}
          loadingCategories={loadingCategories}
          submitting={createProduct.isPending}
        />
      </Paper>
    </Container>
  );
}

export default ProductCreatePage;
