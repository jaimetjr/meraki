import type { Category } from "./category.model";
import type { Benefit } from "./benefit.model";

export interface Service {
  id?: string;
  name: string;
  description: string;
  image: string;
  longDescription: string;
  duration: string;
  price: number;
  currency: string;
  category?: Category;
  benefits: Benefit[];
}