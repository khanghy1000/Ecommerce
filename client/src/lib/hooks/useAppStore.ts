import { create } from 'zustand';

type Store = {
  loadingCount: number;
  addLoading: () => void;
  removeLoading: () => void;

  // Cart selection state
  selectedCartItems: Record<number, boolean>;
  selectCartItem: (productId: number, isSelected: boolean) => void;
  disableAllCartItems: () => void;
  enableCartItem: (productId: number) => void;
};

export const useAppStore = create<Store>((set) => ({
  loadingCount: 0,
  addLoading: () => set((state) => ({ loadingCount: state.loadingCount + 1 })),
  removeLoading: () =>
    set((state) => ({ loadingCount: Math.max(state.loadingCount - 1, 0) })),

  // Cart selection state
  selectedCartItems: {},
  selectCartItem: (productId, isSelected) =>
    set((state) => ({
      selectedCartItems: {
        ...state.selectedCartItems,
        [productId]: isSelected,
      },
    })),
  disableAllCartItems: () =>
    set((state) => ({
      selectedCartItems: Object.keys(state.selectedCartItems).reduce(
        (acc, key) => ({ ...acc, [key]: false }),
        {}
      ),
    })),
  enableCartItem: (productId) =>
    set((state) => ({
      selectedCartItems: {
        ...state.selectedCartItems,
        [productId]: true,
      },
    })),
}));
