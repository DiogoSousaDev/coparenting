export function ErrorMessage({ children }: { children: string }) {
  return (
    <p role="alert" className="mb-4 rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">
      {children}
    </p>
  );
}
