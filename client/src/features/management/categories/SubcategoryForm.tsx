import { useForm } from '@mantine/form';
import { z } from 'zod';
import { zodResolver } from 'mantine-form-zod-resolver';
import { useEffect } from 'react';
import {
  TextInput,
  Button,
  Group,
  Box,
  LoadingOverlay,
  Select,
} from '@mantine/core';
import {
  CategoryResponseDto,
  CreateSubcategoryRequestDto,
} from '../../../lib/types';

const schema = z.object({
  name: z
    .string()
    .min(2, { message: 'Subcategory name must be at least 2 characters' })
    .max(50, { message: 'Subcategory name cannot exceed 50 characters' }),
  categoryId: z
    .number({ message: 'You must select a category' })
    .min(1, { message: 'You must select a category' }),
});

type SubcategoryFormProps = {
  initialValues: CreateSubcategoryRequestDto;
  onSubmit: (values: CreateSubcategoryRequestDto) => void;
  isSubmitting: boolean;
  submitLabel: string;
  categories: CategoryResponseDto[] | undefined;
};

export function SubcategoryForm({
  initialValues,
  onSubmit,
  isSubmitting,
  submitLabel,
  categories = [],
}: SubcategoryFormProps) {
  const form = useForm({
    initialValues,
    validate: zodResolver(schema),
  });

  // Update form values when initialValues change (for edit mode)
  useEffect(() => {
    if (
      initialValues.name !== form.values.name ||
      initialValues.categoryId !== form.values.categoryId
    ) {
      form.setValues(initialValues);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialValues]);

  return (
    <Box pos="relative">
      <LoadingOverlay visible={isSubmitting} />
      <form onSubmit={form.onSubmit(onSubmit)}>
        <Select
          label="Category"
          placeholder="Select a category"
          required
          data={
            categories?.map((category) => ({
              value: category.id.toString(),
              label: category.name,
            })) || []
          }
          value={
            form.values.categoryId > 0 ? form.values.categoryId.toString() : ''
          }
          onChange={(value) =>
            form.setFieldValue('categoryId', value ? parseInt(value) : 0)
          }
          error={form.errors.categoryId}
        />

        <TextInput
          label="Subcategory Name"
          placeholder="Enter subcategory name"
          required
          mt="md"
          {...form.getInputProps('name')}
        />

        <Group justify="flex-end" mt="md">
          <Button type="submit" disabled={isSubmitting}>
            {submitLabel}
          </Button>
        </Group>
      </form>
    </Box>
  );
}
