import { useEffect } from 'react';
import { useAppStore } from './lib/hooks/useAppStore';
import { nprogress } from '@mantine/nprogress';

function Loading() {
  const { loadingCount } = useAppStore();
  useEffect(() => {
    if (loadingCount > 0) {
      nprogress.start();
    } else {
      nprogress.complete();
    }
  }, [loadingCount]);
  return <></>;
}

export default Loading;
