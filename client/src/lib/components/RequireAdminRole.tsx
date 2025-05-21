import { useNavigate } from 'react-router';
import { useAccount } from '../hooks/useAccount';
import { useEffect } from 'react';

function RequireAdminRole({ children }: { children: React.ReactNode }) {
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
    if (currentUserInfo.role !== 'Admin') {
      navigate('/unauthorized');
      return;
    }
  }, [currentUserInfo]);

  if (loadingUserInfo || !currentUserInfo || currentUserInfo.role !== 'Admin') {
    return <></>;
  }

  return <>{children}</>;
}
export default RequireAdminRole;
