import { useState, useEffect } from 'react';
import {
  Container,
  Title,
  Paper,
  Select,
  Group,
  TextInput,
  Button,
  Stack,
  Text,
  Alert,
  Flex,
  SimpleGrid,
  Card,
  Skeleton,
} from '@mantine/core';
import { BarChart } from '@mantine/charts';
import { useStats } from '../../../lib/hooks/useStats';
import { useAccount } from '../../../lib/hooks/useAccount';
import { ShopPerformanceRequest } from '../../../lib/types';
import { IoWarningOutline } from 'react-icons/io5';
import { formatPrice } from '../../../lib/utils';
import { format } from 'date-fns';

function PerformancePage() {
  const { currentUserInfo } = useAccount();
  const isAdmin = currentUserInfo?.role === 'Admin';

  const [shopId, setShopId] = useState<string>('');
  const [metricType, setMetricType] = useState<'Quantity' | 'Value' | 'Orders'>(
    'Value'
  );
  const [timeRange, setTimeRange] = useState<
    'Days' | 'Months' | 'Years' | 'All'
  >('Days');
  const [timeValue, setTimeValue] = useState<number>(30);
  const [performanceRequest, setPerformanceRequest] = useState<
    ShopPerformanceRequest | undefined
  >();

  // Set up initial shopId based on user role
  useEffect(() => {
    if (currentUserInfo) {
      if (currentUserInfo.role === 'Shop') {
        setShopId(currentUserInfo.id);
        setPerformanceRequest({
          shopId: currentUserInfo.id,
          timeRange,
          timeValue,
        });
      }
    }
  }, [currentUserInfo, timeRange, timeValue]);

  const {
    shopPerformance,
    loadingShopPerformance,
    shopSummary,
    loadingShopSummary,
  } = useStats(performanceRequest, performanceRequest?.shopId);

  // Handle applying filters
  const handleApplyFilters = () => {
    if (!shopId) return;

    setPerformanceRequest({
      shopId,
      timeRange,
      timeValue,
    });
  };

  const getChartData = () => {
    if (!shopPerformance) return [];

    return shopPerformance.map((item) => {
      // Format based on metrics
      let displayValue;
      switch (metricType) {
        case 'Value':
          displayValue = item.value;
          break;
        case 'Quantity':
          displayValue = item.quantity;
          break;
        case 'Orders':
          displayValue = item.orderCount;
          break;
        default:
          displayValue = item.value;
      }

      let timeFormat = 'MMMM-yyyy';
      if (timeRange === 'Days') {
        timeFormat = 'dd-MM-yyyy';
      } else if (timeRange === 'Months') {
        timeFormat = 'MMMM-yyyy';
      } else if (timeRange === 'Years') {
        timeFormat = 'yyyy';
      }

      return {
        date: format(new Date(item.time), timeFormat),
        [metricType]: displayValue,
      };
    });
  };

  return (
    <Container size="lg" py="xl">
      <Title order={2} mb="lg">
        Shop Performance
      </Title>

      <Paper p="md" mb="lg" withBorder>
        <Stack gap="md">
          <Group grow>
            {isAdmin && (
              <TextInput
                label="Shop ID"
                placeholder="Enter shop ID"
                value={shopId}
                onChange={(e) => setShopId(e.currentTarget.value)}
              />
            )}

            <Select
              label="Metric Type"
              allowDeselect={false}
              placeholder="Select metric type"
              data={[
                { value: 'Value', label: 'Sales Value' },
                { value: 'Quantity', label: 'Products Sold' },
                { value: 'Orders', label: 'Order Count' },
              ]}
              value={metricType}
              onChange={(value) =>
                setMetricType(value as 'Quantity' | 'Value' | 'Orders')
              }
            />

            <Select
              label="Time Range"
              allowDeselect={false}
              placeholder="Select time range"
              data={[
                { value: 'Days', label: 'Days' },
                { value: 'Months', label: 'Months' },
                { value: 'Years', label: 'Years' },
                { value: 'All', label: 'All Time' },
              ]}
              value={timeRange}
              onChange={(value) =>
                setTimeRange(value as 'Days' | 'Months' | 'Years' | 'All')
              }
            />

            {timeRange !== 'All' && (
              <TextInput
                label={`Number of ${timeRange}`}
                type="number"
                value={String(timeValue)}
                onChange={(e) => setTimeValue(Number(e.currentTarget.value))}
              />
            )}
          </Group>

          <Group justify="flex-end">
            <Button onClick={handleApplyFilters}>Apply Filters</Button>
          </Group>
        </Stack>
      </Paper>

      {!performanceRequest && (
        <Alert
          color="blue"
          title="No Data Selected"
          icon={<IoWarningOutline />}
        >
          {isAdmin
            ? 'Please enter a Shop ID and apply filters to view performance data.'
            : 'Please apply filters to view performance data.'}
        </Alert>
      )}

      {performanceRequest && (
        <>
          <SimpleGrid cols={{ base: 1, sm: 3 }} mb="lg">
            <Card withBorder padding="lg" radius="md">
              {loadingShopSummary ? (
                <Skeleton height={100} />
              ) : (
                <>
                  <Text size="xl" fw={700}>
                    {shopSummary?.totalOrders || 0}
                  </Text>
                  <Text c="dimmed" size="sm">
                    Total Orders
                  </Text>
                </>
              )}
            </Card>
            <Card withBorder padding="lg" radius="md">
              {loadingShopSummary ? (
                <Skeleton height={100} />
              ) : (
                <>
                  <Text size="xl" fw={700}>
                    {shopSummary?.averageRating.toFixed(1) || 'N/A'}
                  </Text>
                  <Text c="dimmed" size="sm">
                    Average Rating
                  </Text>
                </>
              )}
            </Card>
            <Card withBorder padding="lg" radius="md">
              {loadingShopPerformance ? (
                <Skeleton height={100} />
              ) : (
                <>
                  <Text size="xl" fw={700}>
                    {formatPrice(
                      shopPerformance?.reduce(
                        (sum, item) => sum + item.value,
                        0
                      ) || 0
                    )}
                  </Text>
                  <Text c="dimmed" size="sm">
                    Total Revenue
                  </Text>
                </>
              )}
            </Card>
          </SimpleGrid>

          <Paper p="md" withBorder mb="lg">
            <Title order={3} mb="md">
              Performance Over Time
            </Title>
            {loadingShopPerformance ? (
              <Skeleton height={400} />
            ) : shopPerformance && shopPerformance.length > 0 ? (
              <BarChart
                h={400}
                data={getChartData()}
                dataKey="date"
                series={[{ name: metricType, color: 'blue' }]}
              />
            ) : (
              <Flex justify="center" align="center" h={400}>
                <Text c="dimmed">No performance data available</Text>
              </Flex>
            )}
          </Paper>
        </>
      )}
    </Container>
  );
}

export default PerformancePage;
