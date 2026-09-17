import type { ButtonHTMLAttributes } from 'react';

type Variant = 'primary' | 'secondary';

const variantClasses: Record<Variant, string> = {
  primary: 'bg-indigo-600 text-white hover:bg-indigo-700 disabled:bg-indigo-300',
  secondary: 'bg-slate-100 text-slate-700 hover:bg-slate-200 disabled:text-slate-400',
};

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  fullWidth?: boolean;
}

export function Button({ variant = 'primary', fullWidth = true, className = '', ...props }: ButtonProps) {
  return (
    <button
      className={`rounded-md px-4 py-2 text-sm font-medium transition-colors disabled:cursor-not-allowed ${fullWidth ? 'w-full' : ''} ${variantClasses[variant]} ${className}`}
      {...props}
    />
  );
}
