import { useNavigate } from 'react-router';
import { useAccount } from '../hooks/useAccount';
import { useEffect } from 'react';

function RequireBuyerRole({ children }: { children: React.ReactNode }) {
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
    if (currentUserInfo.role !== 'Buyer') {
      navigate('/unauthorized');
      return;
    }
  }, [currentUserInfo]);

  if (loadingUserInfo || !currentUserInfo || currentUserInfo.role !== 'Buyer') {
    return <></>;
  }

  return <>{children}</>;
}
export default RequireBuyerRole;
