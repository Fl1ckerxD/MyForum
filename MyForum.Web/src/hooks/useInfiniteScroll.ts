import { useEffect, useRef, useState } from "react";

interface Options<T> {
  loadMore: (cursor: string | null) => Promise<{
    items: T[];
    nextCursor: string | null;
  }>;
  initialItems?: T[];
  initialCursor?: string | null;
}

export function useInfiniteScroll<T>({
  loadMore,
  initialItems = [],
  initialCursor = null,
}: Options<T>) {
  const [items, setItems] = useState<T[]>(initialItems);
  const [cursor, setCursor] = useState<string | null>(initialCursor);
  //   const [hasMore, setHasMore] = useState(Boolean(initialCursor));
  const [loading, setLoading] = useState(false);

  const loaderRef = useRef<HTMLDivElement | null>(null);

  const load = async () => {
    if (loading || !cursor) return;

    setLoading(true);

    const result = await loadMore(cursor);

    setItems((prev) => [...prev, ...result.items]);
    setCursor(result.nextCursor);
    // setHasMore(Boolean(result.nextCursor));

    setLoading(false);
  };

  useEffect(() => {
    const observer = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting) {
        load();
      }
    });

    if (loaderRef.current) {
      observer.observe(loaderRef.current);
    }

    return () => observer.disconnect();
  }, [cursor]);

  return {
    items,
    setItems,
    setCursor,
    loaderRef,
    loading,
    hasMore: Boolean(cursor),
  };
}
