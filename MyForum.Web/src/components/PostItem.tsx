// import type { Post } from "../types/post"; // Adjust the import based on your types file structure

// interface PostItemProps {
//   post: Post;
//   onDelete: (postId: number) => void;
//   onLike: (postId: number) => void;
//   isAuthenticated: boolean;
// }

// const PostItem = ({
//   post,
//   onDelete,
//   onLike,
//   isAuthenticated,
// }: PostItemProps) => {
//   return (
//     <li>
//       <strong>{post.user.username}</strong>
//       <label className="welcome-text topic-time">
//         {new Date(post.createdAt).toLocaleString()}
//       </label>
//       {isAuthenticated && (
//         <button
//           type="button"
//           className="button-transparent"
//           onClick={() => onDelete(post.id)}
//         >
//           <svg
//             className="delete-button-circle"
//             width="24"
//             height="24"
//             viewBox="0 0 24 24"
//           >
//             <circle cx="12" cy="12" r="10" />
//             <path
//               d="M15 9L9 15M9 9L15 15"
//               stroke="#ffff"
//               strokeWidth="2"
//               strokeLinecap="round"
//             />
//           </svg>
//         </button>
//       )}
//       <p>{post.content}</p>
//       {isAuthenticated ? (
//         <button
//           type="button"
//           className="like-button submit-button"
//           onClick={() => onLike(post.id)}
//         >
//           Понравилось {post.likes.length}
//         </button>
//       ) : (
//         <p>Понравилось {post.likes.length}</p>
//       )}
//     </li>
//   );
// };

// export default PostItem;
