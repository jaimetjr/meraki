import type { IdName } from "./id-name.model";

export interface Therapist {
  id?: string;
  name: string;
  bio: string;
  image?: string | null;
  experience: string;
  education: string;
  proId?: string;
  /** The services this therapist offers. */
  services: IdName[];
}
