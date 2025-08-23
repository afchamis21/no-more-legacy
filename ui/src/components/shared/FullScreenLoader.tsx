import { useEffect, useState } from "react";
import { HashLoader } from "react-spinners";

interface Props {
  messages: string[];
  interval: number;
  random: boolean;
}

export function FullScreenLoader({ messages, interval, random }: Props) {
  const [messageI, setMessageI] = useState<number>(0);

  useEffect(() => {
    if (!messages.length) {
      return;
    }

    const timerId = setInterval(() => {
      setMessageI((currentIndex) => {
        if (random) {
          let nextIndex = currentIndex;
          while (messages.length > 1 && nextIndex === currentIndex) {
            nextIndex = Math.floor(Math.random() * messages.length);
          }
          return nextIndex;
        } else {
          return (currentIndex + 1) % messages.length;
        }
      });
    }, interval);

    return () => clearInterval(timerId);
  }, [messages, interval, random]);

  return (
    <div className="z-90 bg-stone-950/90 text-green-300 absolute inset-0 flex items-center justify-center flex-col gap-4">
      <HashLoader size={64} color="#86efac" />
      {messages.length && <span>{messages[messageI]}</span>}
    </div>
  );
}
