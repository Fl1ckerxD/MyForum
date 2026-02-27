import type { File } from "../types/file";

interface Props {
  files: File[];
  variant?: "list" | "page";
}

export default function PostFilesPreview({ files, variant = "list" }: Props) {
  return (
    <div className="post-files">
      {files.map((file) => (
        <div
          key={file.id}
          className={
            variant === "list" ? "post-file-container" : "post-file-container post-file-middle"
          }
        >
          {file.thumbnailUrl ? (
            <a href={file.fileUrl} target="_blank" rel="noreferrer">
              <img className="post-file" src={file.thumbnailUrl} alt={file.fileName} loading="lazy" />
            </a>
          ) : (
            <a
              href={file.fileUrl}
              target="_blank"
              rel="noreferrer"
              className="file-link"
            >
              {file.fileName}
            </a>
          )}
        </div>
      ))}
    </div>
  );
}
