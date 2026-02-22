import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { Board } from "../../types/board";
import { boardsApi } from "./boardsApi";
import { Icon } from "../../components/Icon";
import { PageHeader } from "../../components/PageHeader";
import { ActionRow } from "../../components/ActionRow";
import "./BoardsPage.css";

type HiddenFilter = "" | "true" | "false";

export const BoardsPage = () => {
  const queryClient = useQueryClient();

  const [name, setName] = useState("");
  const [shortName, setShortName] = useState("");
  const [description, setDescription] = useState("");
  const [editingId, setEditingId] = useState<number | null>(null);
  const [searchName, setSearchName] = useState("");
  const [hiddenFilter, setHiddenFilter] = useState<HiddenFilter>("");

  const { data: boards = [], isLoading } = useQuery<Board[]>({
    queryKey: ["boards"],
    queryFn: boardsApi.getAll,
  });

  const invalidateBoards = () =>
    queryClient.invalidateQueries({ queryKey: ["boards"] });

  const createMutation = useMutation({
    mutationFn: boardsApi.create,
    onSuccess: invalidateBoards,
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: { name: string; shortName: string; description: string } }) =>
      boardsApi.update(id, payload),
    onSuccess: invalidateBoards,
  });

  const deleteMutation = useMutation({
    mutationFn: boardsApi.delete,
    onSuccess: invalidateBoards,
  });

  const visibilityMutation = useMutation({
    mutationFn: ({ id, isHidden }: { id: number; isHidden: boolean }) =>
      boardsApi.patchVisibility(id, isHidden),
    onSuccess: invalidateBoards,
  });

  const isPending = createMutation.isPending || updateMutation.isPending;

  const resetForm = () => {
    setName("");
    setShortName("");
    setDescription("");
    setEditingId(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (editingId) {
      await updateMutation.mutateAsync({
        id: editingId,
        payload: { name, shortName, description },
      });
    } else {
      await createMutation.mutateAsync({ name, shortName, description });
    }

    resetForm();
  };

  const handleEdit = (board: Board) => {
    setName(board.name);
    setShortName(board.shortName);
    setDescription(board.description);
    setEditingId(board.id);
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Удалить эту доску?")) return;
    await deleteMutation.mutateAsync(id);
  };

  const handleToggleVisibility = async (board: Board) => {
    await visibilityMutation.mutateAsync({
      id: board.id,
      isHidden: !board.isHidden,
    });
  };

  const filteredBoards = useMemo(() => {
    const normalizedSearch = searchName.trim().toLowerCase();

    return boards.filter((board) => {
      const matchesName = normalizedSearch.length === 0 || board.name.toLowerCase().includes(normalizedSearch);
      const matchesHidden = hiddenFilter === "" || (hiddenFilter === "true" ? board.isHidden : !board.isHidden);
      return matchesName && matchesHidden;
    });
  }, [boards, searchName, hiddenFilter]);

  return (
    <section className="admin-page boards-page">
      <PageHeader
        icon="boards"
        title="Доски"
        subtitle="Создание, редактирование, скрытие и удаление досок форума"
      />

      <div className="boards-page__grid">
        <form onSubmit={handleSubmit} className="admin-card boards-page__form">
          <h3 className="boards-page__form-title">
            <Icon name={editingId ? "edit" : "plus"} size={16} />
            {editingId ? "Редактирование доски" : "Новая доска"}
          </h3>

          <input className="admin-field" placeholder="Название" value={name} onChange={(e) => setName(e.target.value)} required />
          <input className="admin-field" placeholder="Короткое имя" value={shortName} onChange={(e) => setShortName(e.target.value)} required />
          <textarea className="admin-textarea" placeholder="Описание" value={description} onChange={(e) => setDescription(e.target.value)} rows={4} />

          <ActionRow className="boards-page__actions">
            <button type="submit" className="admin-btn admin-btn-primary" disabled={isPending}>
              <Icon name={editingId ? "edit" : "plus"} size={16} />
              {editingId ? "Обновить" : "Создать"}
            </button>
            {editingId && (
              <button type="button" className="admin-btn admin-btn-neutral" onClick={resetForm}>
                <Icon name="refresh" size={16} />
                Сбросить
              </button>
            )}
          </ActionRow>
        </form>

        <div className="admin-card boards-page__table-card">
          <div className="admin-toolbar">
            <input className="admin-field" placeholder="Поиск по названию" value={searchName} onChange={(e) => setSearchName(e.target.value)} />
            <select className="admin-select" value={hiddenFilter} onChange={(e) => setHiddenFilter(e.target.value as HiddenFilter)}>
              <option value="">Видимость: все</option>
              <option value="false">Только видимые</option>
              <option value="true">Только скрытые</option>
            </select>
          </div>

          {isLoading ? (
            <p>Загрузка...</p>
          ) : filteredBoards.length === 0 ? (
            <div className="admin-empty">Ничего не найдено по текущим фильтрам</div>
          ) : (
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Название</th>
                    <th>Короткое имя</th>
                    <th>Описание</th>
                    <th>Статус</th>
                    <th>Создано</th>
                    <th>Действия</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredBoards.map((board) => (
                    <tr key={board.id} className={board.isHidden ? "boards-page__row-hidden" : undefined}>
                      <td>{board.id}</td>
                      <td>{board.name}</td>
                      <td>/{board.shortName}/</td>
                      <td>{board.description || "-"}</td>
                      <td>
                        <span className={`admin-pill ${board.isHidden ? "admin-pill-warning" : "admin-pill-success"}`}>
                          <Icon name={board.isHidden ? "eyeOff" : "eye"} size={13} />
                          {board.isHidden ? "Скрыта" : "Видима"}
                        </span>
                      </td>
                      <td>{new Date(board.createdAt).toLocaleString()}</td>
                      <td>
                        <ActionRow className="boards-page__row-actions">
                          <button className="admin-btn admin-btn-neutral" onClick={() => handleEdit(board)}>
                            <Icon name="edit" size={14} />
                            Редактировать
                          </button>
                          <button className="admin-btn admin-btn-warning" onClick={() => handleToggleVisibility(board)}>
                            <Icon name={board.isHidden ? "eye" : "eyeOff"} size={14} />
                            {board.isHidden ? "Показать" : "Скрыть"}
                          </button>
                          <button className="admin-btn admin-btn-danger" onClick={() => handleDelete(board.id)}>
                            <Icon name="delete" size={14} />
                            Удалить
                          </button>
                        </ActionRow>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </section>
  );
};
