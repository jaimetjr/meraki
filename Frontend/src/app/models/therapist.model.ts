import type { Specialty } from "./specialty.model";

export interface Therapist {
  id?: string;
  name: string;
  bio: string;
  image?: string | null;
  experience: string;
  education: string;
  proId?: string;
  specialty?: string;
  specialties: Specialty[];
}