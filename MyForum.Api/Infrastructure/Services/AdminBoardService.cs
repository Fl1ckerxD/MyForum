using AutoMapper;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Requests;
using MyForum.Api.Core.Entities;
using MyForum.Api.Core.Interfaces.Repositories;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Infrastructure.Services
{
    public class AdminBoardService : IAdminBoardService
    {
        private readonly ILogger<AdminBoardService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AdminBoardService(ILogger<AdminBoardService> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _uow = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Создает новую доску
        /// </summary>
        /// <returns>Возвращает DTO новой доски</returns>
        /// <exception cref="InvalidOperationException">Если доска с таким коротким именем уже существует</exception>
        public async Task<BoardDto> CreateAsync(CreateBoardRequest request, CancellationToken cancellationToken)
        {
            var existingBoard = await _uow.Boards.GetByShortNameAsync(request.ShortName, cancellationToken);
            if (existingBoard != null)
            {
                _logger.LogWarning("Доска с коротким именем {shortName} уже существует", request.ShortName);
                throw new InvalidOperationException($"Доска с коротким именем {request.ShortName} уже существует");
            }

            Board board = new()
            {
                ShortName = request.ShortName,
                Name = request.Name,
                Description = request.Description
            };

            try
            {
                await _uow.Boards.AddAsync(board, cancellationToken);
                await _uow.SaveAsync(cancellationToken);

                return _mapper.Map<BoardDto>(board);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании доски {@request}", request);
                throw;
            }
        }

        /// <summary>
        /// Удаляет доску по id
        /// </summary>
        /// <returns>Возвращает true, если доска была удалена, иначе false</returns>
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _uow.Boards.DeleteAsync(id, cancellationToken);
                await _uow.SaveAsync(cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении доски с id {id}", id);
                return false;
            }
        }

        /// <summary>
        /// Получает список всех досок
        /// </summary>
        public async Task<IReadOnlyList<BoardDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                var boards = await _uow.Boards.GetAllAsync(cancellationToken);
                return _mapper.Map<IReadOnlyList<BoardDto>>(boards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех досок");
                throw;
            }
        }

        /// <summary>
        /// Обновляет доску по id
        /// </summary>
        /// <returns>Возвращает true, если доска была обновлена</returns>
        /// <exception cref="InvalidOperationException">Если доска с таким коротким именем уже существует</exception>
        public async Task<bool> UpdateAsync(int id, UpdateBoardRequest request, CancellationToken cancellationToken)
        {
            var board = await _uow.Boards.GetByIdAsync(id, cancellationToken);
            if (board == null)
            {
                _logger.LogWarning("Доска с id {id} не найдена", id);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(request.ShortName))
            {
                var existingBoard = await _uow.Boards.GetByShortNameAsync(request.ShortName, cancellationToken);
                if (existingBoard != null && existingBoard.Id != board.Id)
                {
                    _logger.LogWarning("Доска с коротким именем {shortName} уже существует", request.ShortName);
                    throw new InvalidOperationException($"Доска с коротким именем {request.ShortName} уже существует");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.ShortName))
                board.ShortName = request.ShortName;
            if (!string.IsNullOrWhiteSpace(request.Name))
                board.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Description))
                board.Description = request.Description;

            board.IsHidden = request.IsHidden;

            try
            {
                await _uow.SaveAsync(cancellationToken);
                _logger.LogInformation("Доска с id {id} успешно обновлена", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении доски с id {id}", id);
                throw;
            }
        }
    }
}