import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

/**
 * Validates that a string is a valid URL format
 * Returns null if valid, or a validation error object if invalid
 * Allows empty strings (for optional fields)
 */
export function urlValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value || (typeof value === 'string' && value.trim() === "")) {
      return null; // Allow empty values for optional fields
    }

    try {
      new URL(value);
      return null;
    } catch {
      return { url: { value: control.value } };
    }
  };
}

/**
 * Validates that a date is in the future
 * Returns null if valid, or a validation error object if invalid
 * Allows empty strings (for optional fields)
 */
export function futureDateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value || (typeof value === 'string' && value.trim() === "")) {
      return null; // Allow empty values for optional fields
    }

    const inputDate = new Date(value);
    const now = new Date();
    now.setHours(0, 0, 0, 0); // Reset time to start of day for comparison

    if (isNaN(inputDate.getTime())) {
      return { invalidDate: { value: control.value } };
    }

    if (inputDate <= now) {
      return { futureDate: { value: control.value } };
    }

    return null;
  };
}

/**
 * Validates that endDate is greater than or equal to startDate
 * This should be used as a cross-field validator on the form group
 */
export function dateRangeValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const startDate = control.get('startDate')?.value;
    const endDate = control.get('endDate')?.value;

    if (!startDate || !endDate) {
      return null; // Allow empty values
    }

    const start = new Date(startDate);
    const end = new Date(endDate);

    if (isNaN(start.getTime()) || isNaN(end.getTime())) {
      return null; // Let individual field validators handle invalid dates
    }

    if (end < start) {
      return { dateRange: { message: 'Data de término deve ser maior ou igual à data de início' } };
    }

    return null;
  };
}

/**
 * Validates that either image URL or imageFile is provided
 * This should be used as a cross-field validator on the form group
 */
export function imageRequiredValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const imageUrl = control.get('image')?.value;
    const imageFile = control.get('imageFile')?.value;

    const hasUrl = imageUrl && typeof imageUrl === 'string' && imageUrl.trim() !== '';
    const hasFile = imageFile instanceof File;

    if (hasUrl || hasFile) {
      return null; // At least one is provided
    }

    return { imageRequired: { message: 'É necessário fornecer uma URL de imagem ou fazer upload de um arquivo' } };
  };
}

