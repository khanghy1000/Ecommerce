import { useForm } from '@mantine/form';
import { z } from 'zod';
import { zodResolver } from 'mantine-form-zod-resolver';
import { TextInput, Button, Group, Box, LoadingOverlay } from '@mantine/core';
import {
  CreateCategoryRequestDto,
  EditCategoryRequestDto,
} from '../../../lib/types';

const schema = z.object({
  name: z
    .string()
    .min(2, { message: 'Category name must be at least 2 characters' })
    .max(50, { message: 'Category name cannot exceed 50 characters' }),
});

type CategoryFormProps = {
  initialValues: CreateCategoryRequestDto;
  onSubmit: (values: CreateCategoryRequestDto) => void;
  isSubmitting: boolean;
  submitLabel: string;
};

export function CategoryForm({
  initialValues,
  onSubmit,
  isSubmitting,
  submitLabel,
}: CategoryFormProps) {
  const form = useForm({
    initialValues,
    validate: zodResolver(schema),
  });

  return (
    <Box pos="relative">
      <LoadingOverlay visible={isSubmitting} />
      <form onSubmit={form.onSubmit(onSubmit)}>
        <TextInput
          label="Category Name"
          placeholder="Enter category name"
          required
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
