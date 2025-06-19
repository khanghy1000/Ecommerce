import { Card, Image, Text, Badge, Box } from '@mantine/core';
import { ProductResponseDto, PopularProductResponseDto } from '../../lib/types';
import { formatPrice } from '../../lib/utils';
import { Link } from 'react-router';

type ProductCardProps = {
  product: ProductResponseDto | PopularProductResponseDto;
};

export function ProductCard({ product }: ProductCardProps) {
  const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;
  const discountPercent = product.discountPrice
    ? Math.round(
        ((product.regularPrice - product.discountPrice) /
          product.regularPrice) *
          100
      )
    : 0;

  const productId = 'id' in product ? product.id : product.productId;
  const productName = 'name' in product ? product.name : product.productName;

  const mainPhoto =
    product.photos.find((photo) => photo.displayOrder === 1) || null;

  return (
    <Card
      component={Link}
      to={`/products/${productId}`}
      shadow="sm"
      padding="md"
      radius="md"
      withBorder
      style={{ width: 180, textDecoration: 'none' }}
      className="product-card"
      data-product-name={productName}
    >
      {discountPercent > 0 && (
        <Badge
          color="red"
          variant="filled"
          style={{ position: 'absolute', top: 6, right: 6 }}
        >
          -{discountPercent}%
        </Badge>
      )}

      <Box
        style={{
          width: '100%',
          aspectRatio: '1 / 1',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Image
          radius="sm"
          src={
            mainPhoto ? `${baseImageUrl}${mainPhoto.key}` : '/placeholder.svg'
          }
          alt={productName}
          fit="contain"
        />
      </Box>

      <Text
        size="sm"
        mt="xs"
        lineClamp={2}
        style={{ minHeight: '2.6em' }}
        className="product-card-name"
      >
        {productName}
      </Text>

      <Text size="sm" mt={4} style={{ color: 'red', fontWeight: 700 }}>
        {formatPrice(product.discountPrice ?? product.regularPrice)}
      </Text>

      {/* <Text size="xs" color="dimmed" mt={2} hidden>
        Sold {soldCount}
      </Text> */}
    </Card>
  );
}

export default ProductCard;
