import type { File } from "../types/file";
import "../styles/ui/image.css";
import "../styles/layout/section.css";

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
            variant === "list" ? "post-file" : "post-file post-file-middle"
          }
        >
          {file.thumbnailUrl ? (
            <a href={file.fileUrl} target="_blank" rel="noreferrer">
              <img src={file.thumbnailUrl} alt={file.fileName} loading="lazy" />
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
