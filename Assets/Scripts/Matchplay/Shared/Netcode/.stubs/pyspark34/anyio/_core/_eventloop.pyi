from ._compat import DeprecatedAwaitableFloat as DeprecatedAwaitableFloat
from _typeshed import Incomplete
from typing import Any, Awaitable, Callable, Generator, TypeVar

BACKENDS: Incomplete
T_Retval = TypeVar('T_Retval')
threadlocals: Incomplete


def run(func: Callable[..., Awaitable[T_Retval]], *args: object, backend: str = 'asyncio', backend_options: dict[str, Any] | None = None) -> T_Retval:
    """
    Run the given coroutine function in an asynchronous event loop.

    The current thread must not be already running an event loop.

    :param func: a coroutine function
    :param args: positional arguments to ``func``
    :param backend: name of the asynchronous event loop implementation – currently either
        ``asyncio`` or ``trio``
    :param backend_options: keyword arguments to call the backend ``run()`` implementation with
        (documented :ref:`here <backend options>`)
    :return: the return value of the coroutine function
    :raises RuntimeError: if an asynchronous event loop is already running in this thread
    :raises LookupError: if the named backend is not found

    """


async def sleep(delay: float) -> None:
    """
    Pause the current task for the specified duration.

    :param delay: the duration, in seconds

    """


async def sleep_forever() -> None:
    """
    Pause the current task until it's cancelled.

    This is a shortcut for ``sleep(math.inf)``.

    .. versionadded:: 3.1

    """


async def sleep_until(deadline: float) -> None:
    """
    Pause the current task until the given time.

    :param deadline: the absolute time to wake up at (according to the internal monotonic clock of
        the event loop)

    .. versionadded:: 3.1

    """


def current_time() -> DeprecatedAwaitableFloat:
    """
    Return the current value of the event loop's internal clock.

    :return: the clock value (seconds)

    """


def get_all_backends() -> tuple[str, ...]:
    """Return a tuple of the names of all built-in backends."""


def get_cancelled_exc_class() -> type[BaseException]:
    """Return the current async library's cancellation exception class."""


def claim_worker_thread(backend: str) -> Generator[Any, None, None]: ...
def get_asynclib(asynclib_name: str | None = None) -> Any: ...
