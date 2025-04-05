import { useNavigate } from 'react-router';
import useAccount from '../hooks/useAccount';
import { useState } from 'react';

const Login = () => {
  const { loginUser } = useAccount();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const submit = async () => {
    await loginUser.mutateAsync(
      { email, password },
      {
        onSuccess: () => {
          navigate('/');
        },
      }
    );
  };
  return (
    <>
      <div>
        <label htmlFor="email">Email: </label>
        <input
          type="text"
          id="email"
          name="email"
          onChange={(e) => setEmail(e.target.value)}
        />
      </div>
      <div>
        <label htmlFor="password">Password: </label>
        <input
          type="password"
          id="password"
          name="password"
          onChange={(e) => setPassword(e.target.value)}
        />
      </div>
      <button onClick={submit}>Login</button>
    </>
  );
};

export default Login;
