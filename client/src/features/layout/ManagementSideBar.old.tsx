import { Box, Flex, Button, Text } from '@mantine/core';
import { FiPackage, FiArchive, FiTag } from 'react-icons/fi';
import { Link } from 'react-router';

export default function ShopSideBar() {
  return (
    <>
      {/* Navbar */}

      {/* Spacer */}
      <Box
        style={{
          height: 60,
          backgroundColor: '#ffffff',
        }}
      />
      {/* Sidebar */}
      <Box
        component="nav"
        style={{
          position: 'fixed',
          top: 0,
          left: 0,
          width: 220,
          height: '100vh',
          backgroundColor: '#fff',
          borderRight: '1px solid #eaeaea',
          zIndex: 99,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          paddingTop: 70,
          gap: 24,
        }}
      >
        {/* Quản lý đơn hàng */}
        <Box w="100%">
          <Flex align="center" px="md" mb={4} gap={8}>
            <FiPackage size={16} color="#868e96" />
            <Text fw={700} size="sm" c="gray.7">
              Quản lý đơn hàng
            </Text>
          </Flex>
          <Button
            component={Link}
            to="/orders"
            variant="subtle"
            fullWidth
            style={{ justifyContent: 'flex-start', textAlign: 'left' }}
          >
            Tất cả đơn hàng
          </Button>
          <Button
            component={Link}
            to="/orders/pending"
            variant="subtle"
            fullWidth
            style={{ justifyContent: 'flex-start', textAlign: 'left' }}
          >
            Chờ xử lý
          </Button>
          <Button
            component={Link}
            to="/orders/completed"
            variant="subtle"
            fullWidth
            style={{ justifyContent: 'flex-start', textAlign: 'left' }}
          >
            Đã hoàn thành
          </Button>
        </Box>
        {/* Quản lý sản phẩm */}
        <Box w="100%">
          <Flex align="center" px="md" mb={4} gap={8}>
            <FiArchive size={16} color="#868e96" />
            <Text fw={700} size="sm" c="gray.7">
              Quản lý sản phẩm
            </Text>
          </Flex>
          <Button
            component={Link}
            to="/products"
            variant="subtle"
            fullWidth
            style={{ justifyContent: 'flex-start', textAlign: 'left' }}
          >
            Danh sách sản phẩm
          </Button>
          <Button
            component={Link}
            to="/products/add"
            variant="subtle"
            fullWidth
            style={{ justifyContent: 'flex-start', textAlign: 'left' }}
          >
            Thêm sản phẩm
          </Button>
          <Button
            component={Link}
            to="/categories"
            variant="subtle"
            fullWidth
            style={{ justifyContent: 'flex-start', textAlign: 'left' }}
          >
            Danh mục
          </Button>
        </Box>
        {/* Cupon */}
        <Box w="100%">
          <Flex align="center" px="md" mb={4} gap={8}>
            <FiTag size={16} color="#868e96" />
            <Text fw={700} size="sm" c="gray.7">
              Quản lý cupons
            </Text>
          </Flex>
          <Button
            component={Link}
            to="/coupons"
            variant="subtle"
            fullWidth
            style={{ justifyContent: 'flex-start', textAlign: 'left' }}
          >
            Danh sách mã giảm giá
          </Button>
          <Button
            component={Link}
            to="/coupons/add"
            variant="subtle"
            fullWidth
            style={{ justifyContent: 'flex-start', textAlign: 'left' }}
          >
            Tạo mã giảm giá
          </Button>
        </Box>
        {/* Add more sidebar items as needed */}
      </Box>
    </>
  );
}
