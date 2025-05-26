import {
  ActionIcon,
  Badge,
  Button,
  Group,
  Paper,
  Stack,
  Table,
  Text,
  Title,
  Modal,
  ScrollArea,
} from '@mantine/core';
import { useCoupons } from '../../../lib/hooks/useCoupons';
import { useCategories } from '../../../lib/hooks/useCategories';
import { FiEdit, FiTrash2 } from 'react-icons/fi';
import { useState, useEffect } from 'react';
import {
  CouponResponseDto,
  CreateCouponRequestDto,
  EditCouponRequestDto,
} from '../../../lib/types';
import CouponForm from './CouponForm';
import { format } from 'date-fns';
import { formatPrice } from '../../../lib/utils';

function CouponsManagementPage() {
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [selectedCoupon, setSelectedCoupon] =
    useState<CouponResponseDto | null>(null);

  const { coupons, loadingCoupons, createCoupon, editCoupon, deleteCoupon } =
    useCoupons();
  const { categories, loadingCategories } = useCategories();

  // Add debug logging for categories
  useEffect(() => {
    console.log('Categories data loaded:', categories);
  }, [categories]);

  // Log when modals are opened and closed
  useEffect(() => {
    console.log('Create modal state:', createModalOpen);
  }, [createModalOpen]);

  useEffect(() => {
    console.log(
      'Edit modal state:',
      editModalOpen,
      'Selected coupon:',
      selectedCoupon
    );
  }, [editModalOpen, selectedCoupon]);

  const handleCreateClick = () => {
    setCreateModalOpen(true);
  };

  const handleEditClick = (coupon: CouponResponseDto) => {
    setSelectedCoupon(coupon);
    setEditModalOpen(true);
  };

  const handleDeleteClick = (coupon: CouponResponseDto) => {
    setSelectedCoupon(coupon);
    setDeleteModalOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (selectedCoupon) {
      await deleteCoupon.mutateAsync(selectedCoupon.code);
      setDeleteModalOpen(false);
      setSelectedCoupon(null);
    }
  };

  return (
    <Stack>
      <Group justify="space-between">
        <Title order={2}>Coupon Management</Title>
        <Button onClick={handleCreateClick}>Create Coupon</Button>
      </Group>

      <Paper shadow="xs" p="md" withBorder>
        <ScrollArea>
          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Code</Table.Th>
                <Table.Th>Type</Table.Th>
                <Table.Th>Discount</Table.Th>
                <Table.Th>Status</Table.Th>
                <Table.Th>Valid Period</Table.Th>
                <Table.Th>Usage</Table.Th>
                <Table.Th>Actions</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {loadingCoupons ? (
                <Table.Tr>
                  <Table.Td colSpan={7}>
                    <Text ta="center">Loading coupons...</Text>
                  </Table.Td>
                </Table.Tr>
              ) : coupons && coupons.length > 0 ? (
                coupons.map((coupon) => (
                  <Table.Tr key={coupon.code}>
                    <Table.Td>
                      <Text fw={500}>{coupon.code}</Text>
                    </Table.Td>
                    <Table.Td>
                      <Badge
                        color={coupon.type === 'Product' ? 'blue' : 'green'}
                      >
                        {coupon.type}
                      </Badge>
                    </Table.Td>
                    <Table.Td>
                      {coupon.discountType === 'Percent'
                        ? `${coupon.value}%`
                        : `${formatPrice(coupon.value)}`}
                      {coupon.maxDiscountAmount > 0 &&
                        coupon.discountType === 'Percent' &&
                        ` (max ${formatPrice(coupon.maxDiscountAmount)})`}
                    </Table.Td>
                    <Table.Td>
                      <Badge color={coupon.active ? 'green' : 'gray'}>
                        {coupon.active ? 'Active' : 'Inactive'}
                      </Badge>
                    </Table.Td>
                    <Table.Td>
                      {format(new Date(coupon.startTime), 'PPpp')} -{' '}
                      {format(new Date(coupon.endTime), 'PPpp')}
                    </Table.Td>
                    <Table.Td>
                      {coupon.usedCount} /{' '}
                      {coupon.maxUseCount === 0 ? '∞' : coupon.maxUseCount}
                    </Table.Td>
                    <Table.Td>
                      <Group gap="xs">
                        <ActionIcon
                          variant="subtle"
                          color="blue"
                          onClick={() => handleEditClick(coupon)}
                        >
                          <FiEdit size={16} />
                        </ActionIcon>
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={() => handleDeleteClick(coupon)}
                        >
                          <FiTrash2 size={16} />
                        </ActionIcon>
                      </Group>
                    </Table.Td>
                  </Table.Tr>
                ))
              ) : (
                <Table.Tr>
                  <Table.Td colSpan={7}>
                    <Text ta="center">No coupons found</Text>
                  </Table.Td>
                </Table.Tr>
              )}
            </Table.Tbody>
          </Table>
        </ScrollArea>
      </Paper>

      {/* Create Coupon Modal */}
      <Modal
        opened={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        title="Create New Coupon"
        size="lg"
      >
        <CouponForm
          onSubmit={(data: CreateCouponRequestDto) => {
            createCoupon.mutateAsync(data).then(() => {
              setCreateModalOpen(false);
            });
          }}
          categories={categories || []}
          loadingCategories={loadingCategories || false}
        />
      </Modal>

      {/* Edit Coupon Modal */}
      <Modal
        opened={editModalOpen}
        onClose={() => {
          setEditModalOpen(false);
          setSelectedCoupon(null);
        }}
        title="Edit Coupon"
        size="lg"
      >
        {selectedCoupon && (
          <CouponForm
            coupon={selectedCoupon}
            onSubmit={(data: EditCouponRequestDto) => {
              editCoupon
                .mutateAsync({
                  code: selectedCoupon.code,
                  couponRequest: data,
                })
                .then(() => {
                  setEditModalOpen(false);
                  setSelectedCoupon(null);
                });
            }}
            categories={categories || []}
            loadingCategories={loadingCategories || false}
          />
        )}
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal
        opened={deleteModalOpen}
        onClose={() => {
          setDeleteModalOpen(false);
          setSelectedCoupon(null);
        }}
        title="Delete Coupon"
        size="sm"
      >
        <Stack>
          <Text>
            Are you sure you want to delete coupon <b>{selectedCoupon?.code}</b>
            ? This action cannot be undone.
          </Text>
          <Group justify="flex-end">
            <Button
              variant="outline"
              onClick={() => {
                setDeleteModalOpen(false);
                setSelectedCoupon(null);
              }}
            >
              Cancel
            </Button>
            <Button
              color="red"
              onClick={handleDeleteConfirm}
              loading={deleteCoupon.isPending}
            >
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  );
}

export default CouponsManagementPage;
