import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const inteiro: ValidatorFn = (control: AbstractControl): ValidationErrors | null =>
  control.value == null || control.value === '' || Number.isInteger(control.value) ? null : { inteiro: true };

export const multiploDeCentavo: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const valor = control.value;
  if (valor == null || valor === '') return null;
  // Compara em centavos para evitar erro de ponto flutuante (ex.: 0.1 + 0.2)
  const centavos = valor * 100;
  return Math.abs(centavos - Math.round(centavos)) < 1e-6 ? null : { multiploDeCentavo: true };
};

/** Exige valor estritamente maior que `limite`. */
export const maiorQue = (limite: number): ValidatorFn => (control: AbstractControl): ValidationErrors | null =>
  control.value == null || control.value === '' || control.value > limite ? null : { maiorQue: { limite } };

/** Exige valor estritamente menor que `limite`. */
export const menorQue = (limite: number): ValidatorFn => (control: AbstractControl): ValidationErrors | null =>
  control.value == null || control.value === '' || control.value < limite ? null : { menorQue: { limite } };
