import { useEffect, useState } from 'react';
import { useForm, zodResolver } from '@mantine/form';
import {
  TextInput,
  PasswordInput,
  Button,
  Select,
  Paper,
  Title,
  Text,
  Container,
  Stack,
  Anchor,
  Divider,
  Grid,
} from '@mantine/core';
import { z } from 'zod';
import { useAccount } from '../../lib/hooks/useAccount';
import { useLocations } from '../../lib/hooks/useLocations';
import { useNavigate } from 'react-router';
import { notifications } from '@mantine/notifications';
import { LogoOnlyNavbar } from '../layout/LogoOnlyNavbar';
import { FiCheckCircle } from 'react-icons/fi';

const schema = z.object({
  displayName: z.string().min(2, 'Name must have at least 2 characters'),
  email: z.string().email('Invalid email address'),
  password: z
    .string()
    .min(6, 'Password must be at least 6 characters long')
    .regex(/[a-z]/, 'Password must contain a lowercase letter')
    .regex(/[A-Z]/, 'Password must contain an uppercase letter')
    .regex(/\d/, 'Password must contain a digit')
    .regex(
      /[^a-zA-Z0-9]/,
      'Password must contain a non-alphanumeric character'
    ),
  phoneNumber: z.string().min(10, 'Please enter a valid phone number'),
  address: z.string().min(5, 'Address must have at least 5 characters'),
  provinceId: z.number().min(1, 'Please select a province'),
  districtId: z.number().min(1, 'Please select a district'),
  wardId: z.number().min(1, 'Please select a ward'),
  role: z.enum(['Buyer', 'Shop']),
});

type RegisterFormValues = z.infer<typeof schema>;

function RegisterPage() {
  const navigate = useNavigate();
  const { currentUserInfo, loadingUserInfo, registerUser } = useAccount();

  useEffect(() => {
    if (currentUserInfo) {
      navigate('/');
    }
  }, [currentUserInfo, navigate]);

  const [selectedProvinceId, setSelectedProvinceId] = useState<
    number | undefined
  >(undefined);
  const [selectedDistrictId, setSelectedDistrictId] = useState<
    number | undefined
  >(undefined);

  const {
    provinces,
    districts,
    wards,
    loadingProvinces,
    loadingDistricts,
    loadingWards,
  } = useLocations(selectedProvinceId, selectedDistrictId);

  const form = useForm<RegisterFormValues>({
    mode: 'uncontrolled',
    initialValues: {
      displayName: '',
      email: '',
      password: '',
      phoneNumber: '',
      address: '',
      provinceId: 0,
      districtId: 0,
      wardId: 0,
      role: 'Buyer',
    },
    validate: zodResolver(schema),
  });

  const handleProvinceChange = (value: string | null) => {
    if (value) {
      const provinceId = parseInt(value);
      setSelectedProvinceId(provinceId);
      form.setFieldValue('provinceId', provinceId);

      // Reset district and ward selections
      setSelectedDistrictId(undefined);
      form.setFieldValue('districtId', 0);
      form.setFieldValue('wardId', 0);
    }
  };

  const handleDistrictChange = (value: string | null) => {
    if (value) {
      const districtId = parseInt(value);
      setSelectedDistrictId(districtId);
      form.setFieldValue('districtId', districtId);

      // Reset ward selection
      form.setFieldValue('wardId', 0);
    }
  };

  const handleSubmit = form.onSubmit((values) => {
    registerUser.mutate(
      {
        displayName: values.displayName,
        email: values.email,
        password: values.password,
        phoneNumber: values.phoneNumber,
        address: values.address,
        wardId: values.wardId,
        role: values.role,
      },
      {
        onSuccess() {
          notifications.show({
            title: 'Registration successful',
            message: 'You can now log in to your account',
            color: 'green',
            icon: <FiCheckCircle />,
          });
          navigate('/login');
        },
        onError(error) {
          if (Array.isArray(error)) {
            error.forEach((err) => {
              if (err.includes('DisplayName')) {
                form.setFieldError('displayName', err);
              }
              if (err.includes('Email')) {
                form.setFieldError('email', err);
              }
              if (err.includes('Password')) {
                form.setFieldError('password', err);
              }
              if (err.includes('Phone')) {
                form.setFieldError('phoneNumber', err);
              }
              if (err.includes('Address')) {
                form.setFieldError('address', err);
              }
              if (err.includes('Ward')) {
                form.setFieldError('wardId', err);
              }
              if (err.includes('Role')) {
                form.setFieldError('role', err);
              }
            });
          }
        },
      }
    );
  });

  return (
    <>
      <LogoOnlyNavbar />
      <Container size="md" py="xl">
        <Paper radius="md" p="xl" withBorder>
          <Title order={2} ta="center" mb="md">
            Create an account
          </Title>

          <Text size="sm" ta="center" c="dimmed" mb="md">
            Already have an account?{' '}
            <Anchor
              component="button"
              onClick={() => navigate('/login')}
              inherit
            >
              Log in
            </Anchor>
          </Text>

          <Divider my="lg" />

          <form onSubmit={handleSubmit}>
            <Grid gutter="md">
              {/* First column - Personal Information */}
              <Grid.Col span={{ base: 12, sm: 6 }}>
                <Stack gap="md">
                  <TextInput
                    label="Full name"
                    placeholder="Your name"
                    {...form.getInputProps('displayName')}
                  />

                  <TextInput
                    label="Email"
                    placeholder="your@email.com"
                    {...form.getInputProps('email')}
                  />

                  <PasswordInput
                    label="Password"
                    placeholder="Create a password"
                    {...form.getInputProps('password')}
                  />

                  <TextInput
                    label="Phone number"
                    placeholder="Your phone number"
                    type="number"
                    {...form.getInputProps('phoneNumber')}
                  />

                  <Select
                    label="Are you a buyer or seller?"
                    placeholder="Select role"
                    data={[
                      { value: 'Buyer', label: 'I want to shop' },
                      { value: 'Shop', label: 'I want to sell products' },
                    ]}
                    {...form.getInputProps('role')}
                  />
                </Stack>
              </Grid.Col>

              {/* Second column - Address Information */}
              <Grid.Col span={{ base: 12, sm: 6 }}>
                <Stack gap="md">
                  <TextInput
                    label="Address"
                    placeholder="Street address"
                    {...form.getInputProps('address')}
                  />

                  <Select
                    label="Province"
                    placeholder="Select province"
                    data={
                      provinces?.map((province) => ({
                        value: province.id.toString(),
                        label: province.name,
                      })) ?? []
                    }
                    disabled={loadingProvinces}
                    onChange={handleProvinceChange}
                    error={form.errors.provinceId}
                  />

                  <Select
                    label="District"
                    placeholder="Select district"
                    data={
                      districts?.map((district) => ({
                        value: district.id.toString(),
                        label: district.name,
                      })) ?? []
                    }
                    disabled={!selectedProvinceId || loadingDistricts}
                    onChange={handleDistrictChange}
                    error={form.errors.districtId}
                  />

                  <Select
                    label="Ward"
                    placeholder="Select ward"
                    data={
                      wards?.map((ward) => ({
                        value: ward.id.toString(),
                        label: ward.name,
                      })) ?? []
                    }
                    disabled={!selectedDistrictId || loadingWards}
                    onChange={(value) => {
                      if (value) {
                        form.setFieldValue('wardId', parseInt(value));
                      }
                    }}
                    error={form.errors.wardId}
                  />
                </Stack>
              </Grid.Col>

              {/* Button in full width row */}
              <Grid.Col span={12}>
                <Button
                  fullWidth
                  type="submit"
                  loading={registerUser.isPending}
                  disabled={loadingUserInfo}
                >
                  Register
                </Button>
              </Grid.Col>
            </Grid>
          </form>
        </Paper>
      </Container>
    </>
  );
}

export default RegisterPage;
