export const ticksToDate = (ticks) => {
  let ticksSince1970 = ticks - 621355968000000000;
  let milliseconds = ticksSince1970 / 10000;
  return new Date(milliseconds);
}

export const dateToTicks = (date: Date) => {
  const ticksAt1970 = 621355968000000000;
  const unixTimestamp = date.getTime();
  const ticks = unixTimestamp * 10000 + ticksAt1970;
  return ticks;
}
  