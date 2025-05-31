import {
  Container,
  Title,
  Paper,
  Group,
  Button,
  Loader,
  Text,
} from '@mantine/core';
import { Link, useNavigate, useParams } from 'react-router';
import { FiArrowLeft } from 'react-icons/fi';
import { useProducts } from '../../../lib/hooks/useProducts';
import { useCategories } from '../../../lib/hooks/useCategories';
import { EditProductRequestDto } from '../../../lib/types';
import { notifications } from '@mantine/notifications';
import ProductForm from './ProductForm';

function ProductEditPage() {
  const navigate = useNavigate();
  const { productId } = useParams<{ productId: string }>();
  const { product, loadingProduct, editProduct } = useProducts(
    productId ? parseInt(productId, 10) : undefined
  );
  const { categories, loadingCategories } = useCategories();

  const handleSubmit = (data: EditProductRequestDto) => {
    if (!productId) return;

    editProduct.mutate(
      {
        id: parseInt(productId, 10),
        productData: data,
      },
      {
        onSuccess: (updatedProduct) => {
          notifications.show({
            title: 'Product updated',
            message: `Product "${updatedProduct.name}" has been updated successfully`,
            color: 'green',
          });
          navigate('/management/products');
        },
        onError: (error: unknown) => {
          console.error('Error updating product:', error);
          notifications.show({
            title: 'Error',
            message:
              error instanceof Error
                ? error.message
                : 'Failed to update product. Please try again.',
            color: 'red',
          });
        },
      }
    );
  };

  if (loadingProduct || loadingCategories) {
    return (
      <Container size="lg" py="xl">
        <Group justify="center">
          <Loader size="lg" />
          <Text>Loading product...</Text>
        </Group>
      </Container>
    );
  }

  if (!product) {
    return (
      <Container size="lg" py="xl">
        <Group justify="center">
          <Text c="red">Product not found</Text>
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
        Edit Product: {product.name}
      </Title>

      <Paper p="xl" withBorder>
        <ProductForm
          product={product}
          onSubmit={handleSubmit}
          categories={categories || []}
          loadingCategories={loadingCategories}
          submitting={editProduct.isPending}
        />
      </Paper>
    </Container>
  );
}

export default ProductEditPage;
