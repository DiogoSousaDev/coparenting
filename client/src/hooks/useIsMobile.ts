import { useEffect, useState } from 'react';

const DEFAULT_BREAKPOINT_PX = 640;

export function useIsMobile(breakpointPx = DEFAULT_BREAKPOINT_PX) {
  const [isMobile, setIsMobile] = useState(() => window.innerWidth < breakpointPx);

  useEffect(() => {
    const mediaQuery = window.matchMedia(`(max-width: ${breakpointPx - 1}px)`);
    const handleChange = () => setIsMobile(mediaQuery.matches);

    handleChange();
    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, [breakpointPx]);

  return isMobile;
}
