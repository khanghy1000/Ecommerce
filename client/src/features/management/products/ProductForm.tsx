import {
  Button,
  Checkbox,
  Group,
  NumberInput,
  MultiSelect,
  Stack,
  TextInput,
  Grid,
  Box,
  Text,
  Divider,
  Paper,
} from '@mantine/core';
import {
  ProductResponseDto,
  CreateProductRequestDto,
  EditProductRequestDto,
  CategoryResponseDto,
} from '../../../lib/types';
import { useForm, zodResolver } from '@mantine/form';
import { z } from 'zod';
import { RichTextEditor, Link } from '@mantine/tiptap';
import { useEditor } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import { useMemo } from 'react';

interface ProductFormProps {
  product?: ProductResponseDto;
  onSubmit: (data: CreateProductRequestDto | EditProductRequestDto) => void;
  categories: CategoryResponseDto[];
  loadingCategories: boolean;
  submitting?: boolean;
}

const schema = z.object({
  name: z
    .string()
    .min(3, 'Product name must be at least 3 characters')
    .max(200, 'Product name must be at most 200 characters'),
  description: z
    .string()
    .min(10, 'Description must be at least 10 characters')
    .max(5000, 'Description must be at most 5000 characters'),
  regularPrice: z
    .number()
    .positive('Regular price must be positive')
    .min(0.01, 'Regular price must be at least $0.01'),
  quantity: z
    .number()
    .int('Quantity must be a whole number')
    .min(0, 'Quantity cannot be negative'),
  active: z.boolean(),
  length: z
    .number()
    .int('Length must be a whole number')
    .min(1, 'Length must be at least 1 cm'),
  width: z
    .number()
    .int('Width must be a whole number')
    .min(1, 'Width must be at least 1 cm'),
  height: z
    .number()
    .int('Height must be a whole number')
    .min(1, 'Height must be at least 1 cm'),
  weight: z
    .number()
    .int('Weight must be a whole number')
    .min(1, 'Weight must be at least 1 gram'),
  subcategoryIds: z.array(z.number()),
});

function ProductForm({
  product,
  onSubmit,
  categories,
  loadingCategories,
  submitting = false,
}: ProductFormProps) {
  const isEditing = !!product;

  interface FormValues {
    name: string;
    description: string;
    regularPrice: number;
    quantity: number;
    active: boolean;
    length: number;
    width: number;
    height: number;
    weight: number;
    subcategoryIds: number[];
  }

  const form = useForm<FormValues>({
    initialValues: {
      name: product?.name || '',
      description: product?.description || '',
      regularPrice: product?.regularPrice || 0,
      quantity: product?.quantity || 0,
      active: product?.active ?? true,
      length: product?.length || 1,
      width: product?.width || 1,
      height: product?.height || 1,
      weight: product?.weight || 1,
      subcategoryIds: product?.subcategories?.map((subcat) => subcat.id) || [],
    },
    validate: zodResolver(schema),
  });

  // Rich text editor for description
  const editor = useEditor({
    extensions: [StarterKit, Link],
    content: product?.description || '',
    onUpdate: ({ editor }) => {
      form.setFieldValue('description', editor.getHTML());
    },
  });

  // Get subcategory options from all categories
  const subcategoryOptions = useMemo(() => {
    if (!categories || !Array.isArray(categories) || categories.length === 0) {
      return [];
    }

    try {
      const options: { value: string; label: string }[] = [];
      categories.forEach((category) => {
        if (category?.subcategories && Array.isArray(category.subcategories)) {
          category.subcategories.forEach((subcategory) => {
            if (subcategory?.id && subcategory?.name) {
              options.push({
                value: String(subcategory.id),
                label: `${category.name} > ${subcategory.name}`,
              });
            }
          });
        }
      });
      return options;
    } catch (error) {
      console.error('Error processing subcategories:', error);
      return [];
    }
  }, [categories]);

  const handleSubmit = (values: typeof form.values) => {
    // Ensure all fields are properly formatted
    const formData = {
      ...values,
      description: editor?.getHTML() || values.description,
      subcategoryIds: Array.isArray(values.subcategoryIds)
        ? values.subcategoryIds
            .map((id) => Number(id))
            .filter((id) => !isNaN(id))
        : [],
      regularPrice: Number(values.regularPrice),
      quantity: Number(values.quantity),
      length: Number(values.length),
      width: Number(values.width),
      height: Number(values.height),
      weight: Number(values.weight),
    };

    console.log('Product form submission data:', formData);
    onSubmit(formData as CreateProductRequestDto | EditProductRequestDto);
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <Stack gap="lg">
        {/* Basic Information */}
        <Paper p="md" withBorder>
          <Text size="lg" fw={500} mb="md">
            Basic Information
          </Text>
          <Stack gap="md">
            <TextInput
              label="Product Name"
              placeholder="Enter product name"
              description="A clear and descriptive name for your product"
              required
              {...form.getInputProps('name')}
            />

            <Box>
              <Text size="sm" fw={500} mb="xs">
                Description <span style={{ color: 'red' }}>*</span>
              </Text>
              <RichTextEditor editor={editor}>
                <RichTextEditor.Toolbar sticky stickyOffset={60}>
                  <RichTextEditor.ControlsGroup>
                    <RichTextEditor.Bold />
                    <RichTextEditor.Italic />
                    <RichTextEditor.Underline />
                    <RichTextEditor.Strikethrough />
                    <RichTextEditor.ClearFormatting />
                    <RichTextEditor.Code />
                  </RichTextEditor.ControlsGroup>

                  <RichTextEditor.ControlsGroup>
                    <RichTextEditor.H1 />
                    <RichTextEditor.H2 />
                    <RichTextEditor.H3 />
                    <RichTextEditor.H4 />
                  </RichTextEditor.ControlsGroup>

                  <RichTextEditor.ControlsGroup>
                    <RichTextEditor.Blockquote />
                    <RichTextEditor.Hr />
                    <RichTextEditor.BulletList />
                    <RichTextEditor.OrderedList />
                  </RichTextEditor.ControlsGroup>

                  <RichTextEditor.ControlsGroup>
                    <RichTextEditor.Link />
                    <RichTextEditor.Unlink />
                  </RichTextEditor.ControlsGroup>

                  <RichTextEditor.ControlsGroup>
                    <RichTextEditor.Undo />
                    <RichTextEditor.Redo />
                  </RichTextEditor.ControlsGroup>
                </RichTextEditor.Toolbar>

                <RichTextEditor.Content />
              </RichTextEditor>
              {form.errors.description && (
                <Text size="sm" c="red" mt="xs">
                  {form.errors.description}
                </Text>
              )}
            </Box>

            {loadingCategories ? (
              <TextInput
                label="Categories"
                placeholder="Loading categories..."
                disabled
                description="Choose subcategories that best describe your product"
              />
            ) : (
              <MultiSelect
                label="Categories"
                placeholder="Select categories for this product"
                description="Choose subcategories that best describe your product"
                data={subcategoryOptions || []}
                disabled={
                  !subcategoryOptions || subcategoryOptions.length === 0
                }
                searchable
                clearable
                value={form.values.subcategoryIds?.map(String) || []}
                onChange={(values) => {
                  const numericValues = values
                    .map((v) => parseInt(v, 10))
                    .filter((v) => !isNaN(v));
                  form.setFieldValue('subcategoryIds', numericValues);
                }}
                error={form.errors.subcategoryIds}
              />
            )}

            <Checkbox
              label="Active"
              description="Active products are visible to customers"
              {...form.getInputProps('active', { type: 'checkbox' })}
            />
          </Stack>
        </Paper>

        {/* Pricing & Inventory */}
        <Paper p="md" withBorder>
          <Text size="lg" fw={500} mb="md">
            Pricing & Inventory
          </Text>
          <Grid>
            <Grid.Col span={{ base: 12, sm: 6 }}>
              <NumberInput
                label="Regular Price"
                placeholder="1000 ₫"
                description="Price in VND"
                required
                min={1000}
                step={1000}
                suffix=" ₫"
                {...form.getInputProps('regularPrice')}
              />
            </Grid.Col>
            <Grid.Col span={{ base: 12, sm: 6 }}>
              <NumberInput
                label="Quantity"
                placeholder="0"
                description="Available stock"
                required
                min={0}
                {...form.getInputProps('quantity')}
              />
            </Grid.Col>
          </Grid>
        </Paper>

        {/* Dimensions & Weight */}
        <Paper p="md" withBorder>
          <Text size="lg" fw={500} mb="md">
            Dimensions & Weight
          </Text>
          <Text size="sm" c="dimmed" mb="md">
            Required for shipping calculations
          </Text>
          <Grid>
            <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
              <NumberInput
                label="Length (cm)"
                placeholder="0"
                required
                min={1}
                {...form.getInputProps('length')}
              />
            </Grid.Col>
            <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
              <NumberInput
                label="Width (cm)"
                placeholder="0"
                required
                min={1}
                {...form.getInputProps('width')}
              />
            </Grid.Col>
            <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
              <NumberInput
                label="Height (cm)"
                placeholder="0"
                required
                min={1}
                {...form.getInputProps('height')}
              />
            </Grid.Col>
            <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
              <NumberInput
                label="Weight (g)"
                placeholder="0"
                required
                min={1}
                {...form.getInputProps('weight')}
              />
            </Grid.Col>
          </Grid>
        </Paper>

        {/* Placeholder sections */}
        <Paper p="md" withBorder>
          <Text size="lg" fw={500} mb="md">
            Product Photos
          </Text>
          <Text c="dimmed" mb="md">
            Upload and manage product images to showcase your product
          </Text>
          <Button variant="outline" disabled>
            Manage Photos (Coming Soon)
          </Button>
        </Paper>

        <Paper p="md" withBorder>
          <Text size="lg" fw={500} mb="md">
            Discounts
          </Text>
          <Text c="dimmed" mb="md">
            Set up time-limited discounts and promotional pricing
          </Text>
          <Button variant="outline" disabled>
            Manage Discounts (Coming Soon)
          </Button>
        </Paper>

        <Divider />

        {/* Submit buttons */}
        <Group justify="flex-end">
          <Button type="submit" loading={submitting}>
            {isEditing ? 'Update Product' : 'Create Product'}
          </Button>
        </Group>
      </Stack>
    </form>
  );
}

export default ProductForm;
