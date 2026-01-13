import {BaseEntityService} from "~/services/BaseEntityService";
import type {Tag, TagDto, ITagFilter} from "~/entities/entities";

export class TagService extends BaseEntityService<Tag, ITagFilter, TagDto> {
    constructor() {
        super('tag');
    }
}