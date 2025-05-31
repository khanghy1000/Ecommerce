import { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Title,
  Tabs,
  Stack,
  TextInput,
  PasswordInput,
  Button,
  Avatar,
  Group,
  Text,
  FileButton,
  Alert,
  Grid,
  Box,
} from '@mantine/core';
import { useForm, zodResolver } from '@mantine/form';
import { z } from 'zod';
import { useAccount } from '../../lib/hooks/useAccount';
import { notifications } from '@mantine/notifications';
import {
  FiUser,
  FiLock,
  FiCamera,
  FiCheckCircle,
  FiAlertCircle,
} from 'react-icons/fi';

// Validation schemas
const profileSchema = z.object({
  displayName: z.string().min(2, 'Display name must be at least 2 characters'),
});

const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z
    .string()
    .min(6, 'New password must be at least 6 characters long')
    .regex(/[a-z]/, 'Password must contain a lowercase letter')
    .regex(/[A-Z]/, 'Password must contain an uppercase letter')
    .regex(/\d/, 'Password must contain a digit')
    .regex(
      /[^a-zA-Z0-9]/,
      'Password must contain a non-alphanumeric character'
    ),
});

type ProfileFormValues = z.infer<typeof profileSchema>;
type PasswordFormValues = z.infer<typeof passwordSchema>;

function ProfilePage() {
  const baseImageUrl = import.meta.env.VITE_BASE_IMAGE_URL;

  const [activeTab, setActiveTab] = useState<string | null>('profile');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const {
    currentUserInfo,
    loadingUserInfo,
    updateProfile,
    changePassword,
    updateUserImage,
  } = useAccount();

  // Profile form
  const profileForm = useForm<ProfileFormValues>({
    initialValues: {
      displayName: currentUserInfo?.displayName || '',
    },
    validate: zodResolver(profileSchema),
  });

  // Password form
  const passwordForm = useForm<PasswordFormValues>({
    initialValues: {
      currentPassword: '',
      newPassword: '',
    },
    validate: zodResolver(passwordSchema),
  });

  // Update profile form when user data loads
  useEffect(() => {
    if (currentUserInfo) {
      profileForm.setValues({
        displayName: currentUserInfo.displayName,
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUserInfo]);

  const handleProfileSubmit = (values: ProfileFormValues) => {
    updateProfile.mutate(values, {
      onSuccess: async () => {
        notifications.show({
          title: 'Success',
          message: 'Profile updated successfully',
          color: 'green',
          icon: <FiCheckCircle />,
        });
      },
    });
  };

  const handlePasswordSubmit = (values: PasswordFormValues) => {
    changePassword.mutate(values, {
      onSuccess: async () => {
        passwordForm.reset();

        notifications.show({
          title: 'Success',
          message: 'Password changed successfully',
          color: 'green',
          icon: <FiCheckCircle />,
        });
      },
      onError: (error) => {
        console.error('Password change error:', error);
        notifications.show({
          title: 'Error',
          message:
            'Failed to change password. Please check your current password.',
          color: 'red',
          icon: <FiAlertCircle />,
        });
      },
    });
  };

  const handleImageUpload = (file: File | null) => {
    if (file) {
      setSelectedFile(file);
      updateUserImage.mutate(file, {
        onSuccess: async () => {
          setSelectedFile(null);

          notifications.show({
            title: 'Success',
            message: 'Profile image updated successfully',
            color: 'green',
            icon: <FiCheckCircle />,
          });
        },
        onError: (error) => {
          console.error('Image upload error:', error);
          setSelectedFile(null);
          notifications.show({
            title: 'Error',
            message: 'Failed to update profile image. Please try again.',
            color: 'red',
            icon: <FiAlertCircle />,
          });
        },
      });
    }
  };

  if (loadingUserInfo) {
    return (
      <Container size="md" py="xl">
        <Text>Loading...</Text>
      </Container>
    );
  }

  if (!currentUserInfo) {
    return (
      <Container size="md" py="xl">
        <Alert color="red" icon={<FiAlertCircle />}>
          Unable to load user information. Please try refreshing the page.
        </Alert>
      </Container>
    );
  }

  return (
    <Container size="md" py="xl">
      <Title order={1} mb="xl">
        Profile Settings
      </Title>

      <Paper p="xl" withBorder>
        <Tabs value={activeTab} onChange={setActiveTab}>
          <Tabs.List>
            <Tabs.Tab value="profile" leftSection={<FiUser />}>
              Profile Information
            </Tabs.Tab>
            <Tabs.Tab value="password" leftSection={<FiLock />}>
              Change Password
            </Tabs.Tab>
            <Tabs.Tab value="avatar" leftSection={<FiCamera />}>
              Profile Picture
            </Tabs.Tab>
          </Tabs.List>

          <Tabs.Panel value="profile" pt="xl">
            <Stack gap="lg">
              <Box>
                <Title order={3} mb="md">
                  Update Profile Information
                </Title>
                <Text size="sm" c="dimmed" mb="lg">
                  Update your account's profile information.
                </Text>
              </Box>

              <form onSubmit={profileForm.onSubmit(handleProfileSubmit)}>
                <Grid>
                  <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                      label="Display Name"
                      placeholder="Enter your display name"
                      description="This name will be visible to other users"
                      required
                      {...profileForm.getInputProps('displayName')}
                    />
                  </Grid.Col>
                  <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                      label="Email"
                      value={currentUserInfo.email}
                      disabled
                      description="Email cannot be changed"
                    />
                  </Grid.Col>
                  <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                      label="Phone Number"
                      value={currentUserInfo.phoneNumber || 'Not provided'}
                      disabled
                      description="Phone number cannot be changed here"
                    />
                  </Grid.Col>
                  <Grid.Col span={{ base: 12, sm: 6 }}>
                    <TextInput
                      label="Role"
                      value={currentUserInfo.role}
                      disabled
                      description="Account type"
                    />
                  </Grid.Col>
                </Grid>

                <Group justify="flex-end" mt="xl">
                  <Button
                    type="submit"
                    loading={updateProfile.isPending}
                    leftSection={<FiCheckCircle />}
                  >
                    Update Profile
                  </Button>
                </Group>
              </form>
            </Stack>
          </Tabs.Panel>

          <Tabs.Panel value="password" pt="xl">
            <Stack gap="lg">
              <Box>
                <Title order={3} mb="md">
                  Change Password
                </Title>
                <Text size="sm" c="dimmed" mb="lg">
                  Ensure your account is using a long, random password to stay
                  secure.
                </Text>
              </Box>

              <form onSubmit={passwordForm.onSubmit(handlePasswordSubmit)}>
                <Stack gap="md">
                  <PasswordInput
                    label="Current Password"
                    placeholder="Enter your current password"
                    required
                    {...passwordForm.getInputProps('currentPassword')}
                  />

                  <PasswordInput
                    label="New Password"
                    placeholder="Enter your new password"
                    required
                    {...passwordForm.getInputProps('newPassword')}
                  />

                  <Alert color="blue" variant="light">
                    <Text size="sm">Password must contain:</Text>
                    <Text size="xs" c="dimmed" mt="xs">
                      • At least 6 characters
                      <br />
                      • One lowercase letter
                      <br />
                      • One uppercase letter
                      <br />
                      • One digit
                      <br />• One special character
                    </Text>
                  </Alert>
                </Stack>

                <Group justify="flex-end" mt="xl">
                  <Button
                    type="submit"
                    loading={changePassword.isPending}
                    leftSection={<FiLock />}
                  >
                    Change Password
                  </Button>
                </Group>
              </form>
            </Stack>
          </Tabs.Panel>

          <Tabs.Panel value="avatar" pt="xl">
            <Stack gap="lg">
              <Box>
                <Title order={3} mb="md">
                  Profile Picture
                </Title>
                <Text size="sm" c="dimmed" mb="lg">
                  Update your profile picture. This will be visible to other
                  users.
                </Text>
              </Box>

              <Group align="center" gap="xl">
                <Avatar
                  src={baseImageUrl + currentUserInfo.imageUrl}
                  size={120}
                  radius="md"
                >
                  {currentUserInfo.displayName
                    .split(' ')
                    .map((n) => n[0])
                    .join('')
                    .toUpperCase()}
                </Avatar>

                <Stack gap="md">
                  <FileButton
                    onChange={handleImageUpload}
                    accept="image/png,image/jpeg,image/jpg"
                  >
                    {(props) => (
                      <Button
                        {...props}
                        leftSection={<FiCamera />}
                        loading={updateUserImage.isPending}
                        variant="outline"
                      >
                        {selectedFile ? 'Uploading...' : 'Choose New Picture'}
                      </Button>
                    )}
                  </FileButton>

                  {selectedFile && (
                    <Text size="sm" c="dimmed">
                      Selected: {selectedFile.name}
                    </Text>
                  )}

                  <Text size="xs" c="dimmed">
                    JPG, JPEG or PNG.
                  </Text>
                </Stack>
              </Group>
            </Stack>
          </Tabs.Panel>
        </Tabs>
      </Paper>
    </Container>
  );
}

export default ProfilePage;
