import {BaseEntityService} from "~/services/BaseEntityService";
import type {ITag, ITagDto, ITagFilter} from "~/entities/entities";

export class TagService extends BaseEntityService<ITag, ITagFilter, ITagDto> {
    constructor() {
        super('tag');
    }
}