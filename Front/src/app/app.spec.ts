import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';
import { appConfig } from './app.config';

describe('App', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [...appConfig.providers, provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function criar() {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const form = (fixture.componentInstance as any).form;
    return { fixture, form, el: fixture.nativeElement as HTMLElement };
  }

  function submeter(el: HTMLElement) {
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
  }

  it('aceita uma ordem válida', () => {
    const { form } = criar();
    form.setValue({ ativo: 'PETR4', lado: 'C', quantidade: 584, preco: 54.87 });
    expect(form.valid).toBe(true);
  });

  it.each([
    ['quantidade', 0],
    ['quantidade', -1],
    ['quantidade', 1.5],
    ['quantidade', 100_000],
    ['preco', 0],
    ['preco', -0.01],
    ['preco', 1_000],
    ['preco', 10.001],
  ])('rejeita %s = %s', (campo, valor) => {
    const { form } = criar();
    form.controls[campo].setValue(valor);
    expect(form.controls[campo].valid).toBe(false);
  });

  it.each([1, 99_999])('aceita quantidade limite %s', valor => {
    const { form } = criar();
    form.controls.quantidade.setValue(valor);
    expect(form.controls.quantidade.valid).toBe(true);
  });

  it.each([0.01, 0.29, 999.99])('aceita preço %s', valor => {
    const { form } = criar();
    form.controls.preco.setValue(valor);
    expect(form.controls.preco.valid).toBe(true);
  });

  it('não envia formulário inválido e mostra os erros', async () => {
    const { fixture, el } = criar();
    submeter(el);
    await fixture.whenStable();

    httpMock.expectNone('/ordens');
    expect(el.querySelectorAll('.erro').length).toBe(4);
  });

  it('envia a ordem e exibe o sucesso', async () => {
    const { fixture, form, el } = criar();
    form.setValue({ ativo: 'PETR4', lado: 'C', quantidade: 584, preco: 54.87 });
    submeter(el);

    const req = httpMock.expectOne('/ordens');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ ativo: 'PETR4', lado: 'C', quantidade: 584, preco: 54.87 });
    req.flush({ sucesso: true, exposicao_atual: 32044.08, msg_erro: null });
    await fixture.whenStable();

    const resultado = el.querySelector('.resultado')!;
    expect(resultado.querySelector('h2')!.textContent).toContain('Sucesso');
    expect(resultado.textContent).toContain('32.044,08');
  });

  it('exibe o erro retornado pela API em uma rejeição', async () => {
    const { fixture, form, el } = criar();
    form.setValue({ ativo: 'VALE3', lado: 'C', quantidade: 99_999, preco: 999.99 });
    submeter(el);

    httpMock.expectOne('/ordens').flush(
      { sucesso: false, exposicao_atual: 0, msg_erro: 'Limite ultrapassado' },
      { status: 422, statusText: 'Unprocessable Entity' },
    );
    await fixture.whenStable();

    const resultado = el.querySelector('.resultado')!;
    expect(resultado.querySelector('h2')!.textContent).toContain('Erro');
    expect(resultado.textContent).toContain('Limite ultrapassado');
  });

  it('exibe erro de comunicação quando a API está fora do ar', async () => {
    const { fixture, form, el } = criar();
    form.setValue({ ativo: 'VIIA4', lado: 'V', quantidade: 10, preco: 5 });
    submeter(el);

    httpMock.expectOne('/ordens').error(new ProgressEvent('error'));
    await fixture.whenStable();

    expect(el.querySelector('.resultado')!.textContent).toContain('Não foi possível comunicar');
  });
});
