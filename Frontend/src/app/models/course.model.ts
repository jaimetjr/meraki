import { CourseType } from "../enums/course-type.enum";
import { CourseStatus } from "../enums/course-status.enum";
import { Modality } from "../enums/modality.enum";

export interface Course {
  id?: string;
  title: string;
  description: string;
  image: string;
  instructor: string;
  type: CourseType | string;
  status: CourseStatus | string;
  startDate?: string;
  endDate?: string;
  modality?: Modality | string;
  price?: number;
  currency?: string;
  link?: string;
}
