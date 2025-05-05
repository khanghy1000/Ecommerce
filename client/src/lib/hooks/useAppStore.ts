import { create } from 'zustand';

type Store = {
  loadingCount: number;
  addLoading: () => void;
  removeLoading: () => void;
};

export const useAppStore = create<Store>((set) => ({
  loadingCount: 0,
  addLoading: () => set((state) => ({ loadingCount: state.loadingCount + 1 })),
  removeLoading: () =>
    set((state) => ({ loadingCount: Math.max(state.loadingCount - 1, 0) })),
}));
