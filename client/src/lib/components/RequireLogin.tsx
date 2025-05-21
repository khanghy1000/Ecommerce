import { useNavigate } from 'react-router';
import { useAccount } from '../hooks/useAccount';
import { useEffect } from 'react';

function RequireShopRole({ children }: { children: React.ReactNode }) {
  const { currentUserInfo, loadingUserInfo } = useAccount();
  const navigate = useNavigate();

  useEffect(() => {
    if (loadingUserInfo) {
      return;
    }
    if (!currentUserInfo) {
      navigate('/login');
      return;
    }
  }, [currentUserInfo]);

  if (loadingUserInfo || !currentUserInfo) {
    return <></>;
  }

  return <>{children}</>;
}
export default RequireShopRole;
