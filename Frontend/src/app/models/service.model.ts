import type { IdName } from "./id-name.model";

export interface Service {
  id?: string;
  name: string;
  description: string;
  image?: string | null;
  longDescription?: string | null;
  duration?: string | null;
  /** Null when the clinic doesn't publish a price ("sob consulta"). */
  price: number | null;
  currency: string;
  /** One short sentence each, in the order the admin typed them. */
  benefits: string[];
  /** Who offers this service (read-only; set from the therapist's form). */
  therapists: IdName[];
}
