import { Component, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ATIVOS, Ativo, Lado, Ordem, OrdemResposta } from './ordens/ordem.model';
import { OrdemService } from './ordens/ordem.service';
import { inteiro, maiorQue, menorQue, multiploDeCentavo } from './ordens/ordem.validators';

@Component({
  selector: 'app-root',
  imports: [ReactiveFormsModule, CurrencyPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly ordemService = inject(OrdemService);

  protected readonly ativos = ATIVOS;
  protected readonly enviando = signal(false);
  protected readonly resposta = signal<OrdemResposta | null>(null);

  protected readonly form = this.fb.group({
    ativo: this.fb.control<Ativo | ''>('', Validators.required),
    lado: this.fb.control<Lado | ''>('', Validators.required),
    quantidade: this.fb.control<number | null>(null, [Validators.required, inteiro, maiorQue(0), menorQue(100_000)]),
    preco: this.fb.control<number | null>(null, [Validators.required, maiorQue(0), menorQue(1_000), multiploDeCentavo]),
  });

  protected enviar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.ordemService
      .enviar(this.form.getRawValue() as Ordem)
      .pipe(finalize(() => this.enviando.set(false)))
      .subscribe(resposta => this.resposta.set(resposta));
  }

  protected erro(campo: keyof typeof this.form.controls, tipo: string): boolean {
    const control = this.form.controls[campo];
    return control.touched && control.hasError(tipo);
  }
}
