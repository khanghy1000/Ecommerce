import { Anchor, Box, Container, Flex, Group, Image } from "@mantine/core";
import { Link } from "react-router";

export function LogoOnlyNavbar() {
  return (
    <>
      <Box
        component="header"
        py="md"
        style={{
          height: 60,
          position: 'fixed',
          width: '100%',
          zIndex: 1000,
          borderBottom: '1px solid #eaeaea',
        }}
      >
        <Container size="xl" h="100%">
          <Flex align="center" justify="space-between" h="100%">
            {/* Logo / Homepage link */}
            <Anchor component={Link} to="/" underline="never" fw={700}>
              <Group>
                <Image
                  src="/shopee.svg"
                  alt="Shopee"
                  height={30}
                  width="auto"
                />
              </Group>
            </Anchor>
          </Flex>
        </Container>
      </Box>
      <Box
        style={{
          height: 60,
          backgroundColor: '#ffffff',
        }}
      />
    </>
  );
}
