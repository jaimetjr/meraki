export enum CourseStatus {
  EmBreve = 'EmBreve',
  InscricoesAbertas = 'InscricoesAbertas',
  Disponivel = 'Disponivel'
}

export const CourseStatusDisplayNames: Record<CourseStatus, string> = {
  [CourseStatus.EmBreve]: 'Em Breve',
  [CourseStatus.InscricoesAbertas]: 'Inscrições Abertas',
  [CourseStatus.Disponivel]: 'Disponivel'
};

export function getCourseStatusDisplayName(status: CourseStatus | string): string {
  if (status in CourseStatusDisplayNames) {
    return CourseStatusDisplayNames[status as CourseStatus];
  }
  return status;
}

