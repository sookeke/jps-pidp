export class AuthRoutes {
  public static MODULE_PATH = 'auth';

  public static LOGIN = 'login';

  /**
   * @description
   * Useful for redirecting to module root-level routes.
   */
  public static routePath(route: string): string {
    return `/${AuthRoutes.MODULE_PATH}/${route}`;
  }
}
