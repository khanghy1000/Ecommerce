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
    if (currentUserInfo.role !== 'Shop') {
      navigate('/unauthorized');
      return;
    }
  }, [currentUserInfo]);

  if (loadingUserInfo || !currentUserInfo || currentUserInfo.role !== 'Shop') {
    return <></>;
  }

  return <>{children}</>;
}
export default RequireShopRole;
