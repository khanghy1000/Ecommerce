import { useForm, zodResolver } from '@mantine/form';
import { z } from 'zod';
import { useAccount } from '../../lib/hooks/useAccount';
import { useEffect } from 'react';
import { useNavigate } from 'react-router';
import {
  TextInput,
  PasswordInput,
  Button,
  Paper,
  Title,
  Text,
  Container,
  Group,
  Anchor,
  Divider,
  Stack,
} from '@mantine/core';

const loginSchema = z.object({
  email: z.string().email({ message: 'Invalid email address' }),
  password: z
    .string()
    .min(6, { message: 'Password should be at least 6 characters' }),
});

function LoginPage() {
  const navigate = useNavigate();
  const { loginUser, currentUserInfo, loadingUserInfo } = useAccount();

  useEffect(() => {
    if (currentUserInfo) {
      navigate('/');
    }
  }, [currentUserInfo, navigate]);

  const form = useForm({
    initialValues: {
      email: '',
      password: '',
    },
    validate: zodResolver(loginSchema),
  });

  const handleSubmit = form.onSubmit((values) => {
    loginUser.mutate(values, {
      onSuccess: () => {
        navigate('/');
      },
      onError: () => {
        form.setErrors({
          password:
            'Login failed. Please check your credentials and try again.',
        });
      },
    });
  });

  return (
    <Container size="xs" my={40}>
      <Paper radius="md" p="xl" withBorder>
        <Title ta="center" order={2} mb="md">
          Welcome back
        </Title>
        <Text c="dimmed" size="sm" ta="center" mb="lg">
          Don't have an account yet?{' '}
          <Anchor
            component="button"
            onClick={() => navigate('/register')}
            inherit
          >
            Create account
          </Anchor>
        </Text>

        <Divider my="lg" />

        <form onSubmit={handleSubmit}>
          <Stack gap="md">
            <TextInput
              required
              label="Email"
              placeholder="your@email.com"
              {...form.getInputProps('email')}
            />

            <PasswordInput
              required
              label="Password"
              placeholder="Your password"
              {...form.getInputProps('password')}
            />

            <Group justify="space-between" mt="sm">
              <Anchor
                size="sm"
                onClick={() => navigate('/forgot-password')}
              >
                Forgot password?
              </Anchor>
            </Group>

            <Button
              fullWidth
              type="submit"
              loading={loginUser.isPending}
              disabled={loadingUserInfo}
            >
              Log in
            </Button>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
}

export default LoginPage;
