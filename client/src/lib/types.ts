export type ErrorResponse = {
  detail?: string;
  status?: number;
  title?: string;
  type?: string;
  errors?: {
    [key: string]: string[];
  };
};
