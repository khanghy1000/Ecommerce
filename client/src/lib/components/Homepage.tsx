import { useNavigate } from 'react-router';
import useAccount from '../hooks/useAccount';
import { useEffect } from 'react';

const Homepage = () => {
  const { userInfo, isLoadingUserInfo } = useAccount();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isLoadingUserInfo && !userInfo) {
      navigate('/login');
    }
  }, [isLoadingUserInfo, userInfo, navigate]);

  if (isLoadingUserInfo) {
    return <div>Loading...</div>;
  }

  return (
    <div>
      <pre>{JSON.stringify(userInfo, null, 2)}</pre>
    </div>
  );
};

export default Homepage;
