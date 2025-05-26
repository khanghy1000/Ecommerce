import {
  Button,
  Checkbox,
  Group,
  NumberInput,
  Select,
  Stack,
  TextInput,
  Flex,
  Divider,
  Text,
} from '@mantine/core';
import {
  CategoryResponseDto,
  CouponResponseDto,
  CreateCouponRequestDto,
  EditCouponRequestDto,
} from '../../../lib/types';
import { useForm, zodResolver } from '@mantine/form';
import { z } from 'zod';
import { DateTimePicker } from '@mantine/dates';

interface CouponFormProps {
  coupon?: CouponResponseDto;
  onSubmit: (data: CreateCouponRequestDto | EditCouponRequestDto) => void;
  categories: CategoryResponseDto[];
  loadingCategories: boolean;
}

const schema = z.object({
  code: z
    .string()
    .min(3, 'Code must be at least 3 characters')
    .max(20, 'Code must be at most 20 characters'),
  active: z.boolean(),
  startTime: z.date(),
  endTime: z.date(),
  type: z.enum(['Product', 'Shipping']),
  discountType: z.enum(['Percent', 'Amount']),
  value: z.number().positive('Value must be positive'),
  minOrderValue: z.number().min(0, 'Min order value cannot be negative'),
  maxDiscountAmount: z
    .number()
    .min(0, 'Max discount amount cannot be negative'),
  allowMultipleUse: z.boolean(),
  maxUseCount: z.number().int().min(0, 'Max use count cannot be negative'),
  categoryIds: z.array(z.number()),
});

function CouponForm({
  coupon,
  onSubmit,
  categories,
  loadingCategories,
}: CouponFormProps) {
  const isEditing = !!coupon;

  interface FormValues {
    code: string;
    active: boolean;
    startTime: Date;
    endTime: Date;
    type: 'Product' | 'Shipping';
    discountType: 'Percent' | 'Amount';
    value: number;
    minOrderValue: number;
    maxDiscountAmount: number;
    allowMultipleUse: boolean;
    maxUseCount: number;
    categoryIds: number[];
  }

  // Parse dates safely
  const parseDate = (
    dateString: string | Date | undefined,
    defaultDate: Date
  ): Date => {
    if (!dateString) return defaultDate;
    try {
      const date = new Date(dateString);
      return isNaN(date.getTime()) ? defaultDate : date;
    } catch {
      // Return default if any error occurs during date parsing
      return defaultDate;
    }
  };

  const now = new Date();
  const oneWeekLater = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

  const form = useForm<FormValues>({
    initialValues: {
      code: coupon?.code || '',
      active: coupon?.active ?? true,
      startTime: parseDate(coupon?.startTime, now),
      endTime: parseDate(coupon?.endTime, oneWeekLater),
      type: (coupon?.type as 'Product' | 'Shipping') || 'Product',
      discountType: (coupon?.discountType as 'Percent' | 'Amount') || 'Percent',
      value: typeof coupon?.value === 'number' ? coupon.value : 10,
      minOrderValue:
        typeof coupon?.minOrderValue === 'number' ? coupon.minOrderValue : 0,
      maxDiscountAmount:
        typeof coupon?.maxDiscountAmount === 'number'
          ? coupon.maxDiscountAmount
          : 0,
      allowMultipleUse: coupon?.allowMultipleUse ?? true,
      maxUseCount:
        typeof coupon?.maxUseCount === 'number' ? coupon.maxUseCount : 0,
      categoryIds: [], // Initialize as empty array - will be populated from category selection
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = (values: typeof form.values) => {
    // Ensure all fields are properly formatted
    const formData = {
      ...values,
      // Ensure categoryIds is a valid array of numbers (filtering out any invalid values)
      categoryIds: Array.isArray(values.categoryIds)
        ? values.categoryIds.filter(
            (id) => typeof id === 'number' && !isNaN(id)
          )
        : [],
      startTime: values.startTime,
      endTime: values.endTime,
      // Ensure numeric values are proper numbers
      value: Number(values.value),
      minOrderValue: Number(values.minOrderValue),
      maxDiscountAmount: Number(values.maxDiscountAmount),
      maxUseCount: Number(values.maxUseCount),
    };

    console.log('Form submission data:', formData);

    if (isEditing) {
      // When editing, we omit the code field since it can't be changed
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      const { code, ...editData } = formData;
      onSubmit(editData as EditCouponRequestDto);
    } else {
      onSubmit(formData as CreateCouponRequestDto);
    }
  };

  // Function to get select options with fallback for safety
  const getSelectData = () => {
    // Default option as fallback
    const defaultOptions = [{ value: '0', label: 'All Categories' }];

    // Return default if no categories are available
    if (!categories || !Array.isArray(categories) || categories.length === 0) {
      return defaultOptions;
    }

    try {
      // Safe mapping of category data
      const options = categories.map((category) => ({
        value: String(category.id),
        label: category.name,
        group: 'Categories',
      }));

      return options.length > 0 ? options : defaultOptions;
    } catch (error) {
      console.error('Error processing categories:', error);
      return defaultOptions;
    }
  };

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <Stack>
        <TextInput
          label="Coupon Code"
          placeholder="Enter a unique coupon code (e.g. SUMMER25)"
          description="Unique identifier for this coupon"
          required
          {...form.getInputProps('code')}
          disabled={isEditing}
        />

        <Flex gap="md" wrap="wrap" mb="md">
          <DateTimePicker
            label="Start Time"
            placeholder="Select start date and time"
            required
            style={{ flex: '1 0 260px' }}
            valueFormat="MMM D, YYYY HH:mm"
            clearable={false}
            firstDayOfWeek={0}
            dropdownType="popover"
            {...form.getInputProps('startTime')}
          />

          <DateTimePicker
            label="End Time"
            placeholder="Select end date and time"
            required
            style={{ flex: '1 0 260px' }}
            valueFormat="MMM D, YYYY HH:mm"
            clearable={false}
            firstDayOfWeek={0}
            dropdownType="popover"
            minDate={form.values.startTime}
            {...form.getInputProps('endTime')}
          />
        </Flex>

        <Flex gap="md">
          <Select
            label="Coupon Type"
            placeholder="Select type"
            required
            data={[
              { value: 'Product', label: 'Product Discount' },
              { value: 'Shipping', label: 'Shipping Discount' },
            ]}
            style={{ flex: 1 }}
            {...form.getInputProps('type')}
          />

          <Select
            label="Discount Type"
            placeholder="Select discount type"
            required
            data={[
              { value: 'Percent', label: 'Percentage (%)' },
              { value: 'Amount', label: 'Fixed Amount ($)' },
            ]}
            style={{ flex: 1 }}
            {...form.getInputProps('discountType')}
          />
        </Flex>

        <Flex gap="md" wrap="wrap">
          <NumberInput
            label="Discount Value"
            placeholder={
              form.values.discountType === 'Percent'
                ? 'Enter percentage'
                : 'Enter amount'
            }
            description={
              form.values.discountType === 'Percent'
                ? 'Percentage discount (1-100)'
                : 'Fixed amount discount'
            }
            required
            min={0}
            max={form.values.discountType === 'Percent' ? 100 : undefined}
            style={{ flex: 1 }}
            {...form.getInputProps('value')}
          />

          <NumberInput
            label="Min Order Value"
            placeholder="Minimum order value"
            description="Minimum cart value required to use this coupon"
            min={0}
            style={{ flex: 1 }}
            {...form.getInputProps('minOrderValue')}
          />
        </Flex>

        {form.values.discountType === 'Percent' && (
          <NumberInput
            label="Max Discount Amount"
            placeholder="Maximum discount amount (0 for unlimited)"
            description="Maximum amount that can be discounted (0 = unlimited)"
            min={0}
            {...form.getInputProps('maxDiscountAmount')}
          />
        )}

        <Divider my="sm" />
        <Text fw={500}>Usage Limits</Text>

        <Checkbox
          label="Allow multiple use per customer"
          {...form.getInputProps('allowMultipleUse', { type: 'checkbox' })}
        />

        <NumberInput
          label="Maximum Use Count"
          placeholder="Maximum number of uses (0 for unlimited)"
          min={0}
          {...form.getInputProps('maxUseCount')}
        />

        <Divider my="sm" />
        <Text fw={500}>Category Restrictions</Text>

        {/* Simplified category selection */}
        <Select
          label="Applicable Category"
          placeholder="Select a category"
          description="Choose a category to apply this coupon to"
          data={getSelectData()}
          clearable
          disabled={loadingCategories}
          value={
            form.values.categoryIds?.length > 0
              ? String(form.values.categoryIds[0])
              : null
          }
          onChange={(value) => {
            const categoryId = parseInt(value || '0', 10);
            form.setFieldValue(
              'categoryIds',
              categoryId > 0 ? [categoryId] : []
            );
          }}
        />

        <Checkbox
          label="Active"
          {...form.getInputProps('active', { type: 'checkbox' })}
        />

        <Group justify="flex-end" mt="xl">
          <Button
            variant="outline"
            onClick={(e) => {
              e.preventDefault();
              // This will trigger the modal's onClose handler
              const closeButton = document.querySelector(
                '.mantine-Modal-close'
              );
              if (closeButton instanceof HTMLElement) {
                closeButton.click();
              }
            }}
          >
            Cancel
          </Button>
          <Button type="submit" color="blue">
            {isEditing ? 'Update Coupon' : 'Create Coupon'}
          </Button>
        </Group>
      </Stack>
    </form>
  );
}

export default CouponForm;
