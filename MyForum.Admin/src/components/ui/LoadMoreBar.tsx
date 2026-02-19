import { Icon } from "./Icon";

type LoadMoreBarProps = {
  hasNextPage?: boolean;
  isFetching: boolean;
  onLoadMore: () => void;
  loadMoreText?: string;
  loadingText?: string;
};

export const LoadMoreBar = ({
  hasNextPage,
  isFetching,
  onLoadMore,
  loadMoreText = "Загрузить еще",
  loadingText = "Загрузка...",
}: LoadMoreBarProps) => {
  return (
    <div className="ui-load-more-bar">
      {hasNextPage && (
        <button className="admin-btn admin-btn-primary" onClick={onLoadMore}>
          <Icon name="plus" size={14} />
          {loadMoreText}
        </button>
      )}
      {isFetching && <span>{loadingText}</span>}
    </div>
  );
};
