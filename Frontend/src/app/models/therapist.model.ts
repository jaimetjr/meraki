import type { Specialty } from "./specialty.model";

export interface Therapist {
  id?: string;
  name: string;
  bio: string;
  image: string;
  experience: string;
  education: string;
  specialty?: string;
  specialties: Specialty[];
}