import {
  Box,
  Button,
  Center,
  Container,
  Group,
  Stack,
  Text,
  Title,
} from '@mantine/core';
import { FiAlertCircle, FiArrowLeft } from 'react-icons/fi';
import { Link } from 'react-router';

function Unauthorized() {
  return (
    <Container size="md">
      <Center mih="80vh">
        <Stack align="center" gap="xl">
          <FiAlertCircle size={80} color="var(--mantine-color-shopee-5)" />

          <Box ta="center">
            <Title order={1} size="2.5rem" mb="md">
              403 - Unauthorized
            </Title>
            <Text size="lg" c="dimmed" maw={500} mx="auto">
              You do not have permission to access this page.
            </Text>
          </Box>

          <Group>
            <Button
              component={Link}
              to="/"
              size="md"
              leftSection={<FiArrowLeft size={16} />}
            >
              Back to Home
            </Button>
          </Group>
        </Stack>
      </Center>
    </Container>
  );
}

export default Unauthorized;
