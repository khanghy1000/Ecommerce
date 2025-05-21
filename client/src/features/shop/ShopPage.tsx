import { useParams } from 'react-router';

function ShopPage() {
  const { shopId } = useParams();
  return <div>{shopId}</div>;
}

export default ShopPage;
